using System.Diagnostics;
using System.Runtime.Versioning;
using Hardware.Info;

namespace backdoor.services;

public partial class SysMonitor
{
    private void UpdateDiskInfo()
    {
        try
        {
            if (OperatingSystem.IsLinux())
            {
                UpdateDiskInfoForLinuxWorkload();
                return;
            }

            if (!OperatingSystem.IsWindows())
            {
                UpdateDiskInfoForUnixLike();
                return;
            }

            UpdateDiskInfoForWindows();
        }
        catch
        {
            DiskUsage = [new DiskMetric("Disk", "Unknown", "N/A")];
        }
    }

    private void UpdateDiskInfoForLinuxWorkload()
    {
        var now = DateTimeOffset.UtcNow;
        var currentTotalIoTimeMs = ReadLinuxTotalIoTimeMs();

        if (currentTotalIoTimeMs is null)
        {
            DiskUsage = [new DiskMetric("Disk", "I/O busy", "N/A")];
            return;
        }

        if (linuxTotalIoTimeMs is null || linuxDiskSampleTimestamp is null)
        {
            linuxTotalIoTimeMs = currentTotalIoTimeMs.Value;
            linuxDiskSampleTimestamp = now;
            DiskUsage = [new DiskMetric("Disk (All)", "I/O busy", "N/A")];
            return;
        }

        var elapsedMs = (now - linuxDiskSampleTimestamp.Value).TotalMilliseconds;
        if (elapsedMs <= 0)
        {
            DiskUsage = [new DiskMetric("Disk", "I/O busy", "N/A")];
            linuxTotalIoTimeMs = currentTotalIoTimeMs.Value;
            linuxDiskSampleTimestamp = now;
            return;
        }

        var deltaIoMs = Math.Max(0, currentTotalIoTimeMs.Value - linuxTotalIoTimeMs.Value);
        var busyPercent = Math.Clamp(deltaIoMs / elapsedMs * 100d, 0d, 100d);

        DiskUsage = [new DiskMetric("Disk (All)", "I/O busy", $"{busyPercent:0.#}%")];
        linuxTotalIoTimeMs = currentTotalIoTimeMs.Value;
        linuxDiskSampleTimestamp = now;
    }

    private static long? ReadLinuxTotalIoTimeMs()
    {
        if (!File.Exists("/proc/diskstats"))
        {
            return null;
        }

        long totalIoTimeMs = 0;
        var found = false;

        foreach (var line in File.ReadLines("/proc/diskstats"))
        {
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 12)
            {
                continue;
            }

            var name = parts[2];
            if (!IsLinuxRootDisk(name))
            {
                continue;
            }

            if (!long.TryParse(parts[12], out var ioTimeMs))
            {
                continue;
            }

            totalIoTimeMs += ioTimeMs;
            found = true;
        }

        return found ? totalIoTimeMs : null;
    }

    private static bool IsLinuxRootDisk(string name)
    {
        if (name.StartsWith("nvme", StringComparison.Ordinal) && name.Contains('n') && !name.Contains('p'))
        {
            return true;
        }

        if (name.StartsWith("mmcblk", StringComparison.Ordinal) && !name.Contains('p'))
        {
            return true;
        }

        if ((name.Length == 3 &&
             (name.StartsWith("sd", StringComparison.Ordinal) ||
              name.StartsWith("vd", StringComparison.Ordinal) ||
              name.StartsWith("hd", StringComparison.Ordinal))) ||
            (name.Length == 4 && name.StartsWith("xvd", StringComparison.Ordinal)))
        {
            return true;
        }

        return false;
    }

    [SupportedOSPlatform("windows")]
    private void UpdateDiskInfoForWindows()
    {
        hardwareInfo.RefreshDriveList();

        if (diskCountersByName is null)
        {
            diskCountersByName = new Dictionary<string, PerformanceCounter>();

            var category = new PerformanceCounterCategory("PhysicalDisk");
            var instances = category.GetInstanceNames()
                .Where(name => !string.Equals(name, "_Total", StringComparison.OrdinalIgnoreCase));
            foreach (var instance in instances)
            {
                var counter = new PerformanceCounter("PhysicalDisk", "% Disk Time", instance, true);
                _ = counter.NextValue();
                diskCountersByName[instance] = counter;
            }
        }

        var disks = diskCountersByName
            .OrderBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kvp =>
            {
                var value = Math.Clamp(kvp.Value.NextValue(), 0f, 100f);
                var drive = FindDrive(kvp.Key);
                var diskLabel = BuildDiskLabel(drive, kvp.Key);
                return new DiskMetric(diskLabel, string.Empty, $"{value:0.#}%");
            })
            .ToList();

        DiskUsage = disks.Count > 0 ? disks : [new DiskMetric("Disk", "Disk", "N/A")];
    }

    private void UpdateDiskInfoForUnixLike()
    {
        var disks = DriveInfo.GetDrives()
            .Where(drive => drive.IsReady && drive.TotalSize > 0)
            .OrderBy(drive => drive.Name, StringComparer.OrdinalIgnoreCase)
            .Select(drive =>
            {
                var used = Math.Clamp((double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize * 100, 0d, 100d);
                var name = string.IsNullOrWhiteSpace(drive.Name) ? "Disk" : drive.Name;
                var type = string.IsNullOrWhiteSpace(drive.DriveFormat) ? "Filesystem" : drive.DriveFormat;
                return new DiskMetric(name, type, $"{used:0.#}%");
            })
            .ToList();

        DiskUsage = disks.Count > 0 ? disks : [new DiskMetric("Disk", "Filesystem", "N/A")];
    }

    private Drive? FindDrive(string instanceName)
    {
        string indexToken = instanceName.Split(' ')[0];
        if (indexToken == null)
        {
            return null;
        }

        int.TryParse(indexToken, out int index);
        return hardwareInfo.DriveList.FirstOrDefault(d => d.Index == index);
    }

    private static string BuildDiskLabel(Drive? drive, string fallback)
    {
        if (drive is null)
        {
            return $"Disk ({fallback})";
        }

        var model = string.IsNullOrWhiteSpace(drive.Model) ? "Unknown Drive" : drive.Model;
        return $"Disk {drive.Index} ({model})";
    }
}
