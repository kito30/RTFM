using System.Diagnostics;
using Hardware.Info;

namespace backdoor.services;

public class SysMonitor : ISysMonitor
{
    public string CpuUsage { get; private set; }
    public string GpuUsage { get; private set; }
    public string MemoryUsage { get; private set; }
    public IReadOnlyList<DiskMetric> DiskUsage { get; private set; }
    public string OS { get; private set; }

    private readonly IHardwareInfo hardwareInfo;
    private Dictionary<string, PerformanceCounter>? diskCountersByName;

    public SysMonitor(IHardwareInfo hardwareInfo)
    {
        CpuUsage = "0%";
        GpuUsage = "N/A";
        MemoryUsage = "0%";
        DiskUsage = [];
        OS = Environment.OSVersion.ToString();
        this.hardwareInfo = hardwareInfo;

        UpdateOperatingSystemInfo();
        UpdateSystemInfo();
    }

    public void UpdateSystemInfo()
    {
        UpdateCpuInfo();
        UpdateMemoryInfo();
        UpdateDiskInfo();
        UpdateGpuInfo();
        UpdateOperatingSystemInfo();
    }

    private void UpdateCpuInfo()
    {
        try
        {
            hardwareInfo.RefreshCPUList();

            var cpu = hardwareInfo.CpuList.FirstOrDefault();
            if (cpu is null)
            {
                CpuUsage = "N/A";
                return;
            }

            var cpuPercent = cpu.CpuCoreList.Count > 0
                ? cpu.CpuCoreList.Average(core => (double)core.PercentProcessorTime)
                : cpu.PercentProcessorTime;

            CpuUsage = $"{cpuPercent:0.#}%";
        }
        catch
        {
            CpuUsage = "N/A";
        }
    }

    private void UpdateMemoryInfo()
    {
        try
        {
            hardwareInfo.RefreshMemoryStatus();

            var totalMemory = hardwareInfo.MemoryStatus.TotalPhysical;
            if (totalMemory == 0)
            {
                MemoryUsage = "N/A";
                return;
            }

            var availableMemory = Math.Min(hardwareInfo.MemoryStatus.AvailablePhysical, totalMemory);
            var usedMemory = totalMemory - availableMemory;
            MemoryUsage = $"{(double)usedMemory / totalMemory * 100:0.#}%";
        }
        catch
        {
            MemoryUsage = "N/A";
        }
    }

    private void UpdateDiskInfo()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                DiskUsage = [new DiskMetric("Disk", "Unsupported", "N/A")];
                return;
            }

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
        catch
        {
            DiskUsage = [new DiskMetric("Disk", "Unknown", "N/A")];
        }
    }

    private Drive? FindDrive(string instanceName)
    {
        // Splits string into parts using space ' ', gets the first part which is the drives name.
        // The format will be like " Disk name + C: or the drives partition name
        string indexToken = instanceName.Split(' ')[0];
        if(indexToken == null)
        {
            return null;
        }
        int.TryParse(indexToken, out int index);
        // hmm, basically fucking windows doesnt get the name of the hard drive
        // but just return the disk index like 0 or 1 so we search it with DriveList
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

    private void UpdateGpuInfo()
    {
        GpuUsage = "N/A";
    }

    private void UpdateOperatingSystemInfo()
    {
        try
        {
            hardwareInfo.RefreshOperatingSystem();
            OS = hardwareInfo.OperatingSystem?.Name ?? OS;
        }
        catch
        {
            // Keep last known OS value.
        }
    }
}
