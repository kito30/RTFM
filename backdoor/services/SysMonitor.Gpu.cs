using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text.Json;
using Hardware.Info;

namespace backdoor.services;

public partial class SysMonitor
{
    private void UpdateGpuInfo()
    {
        try
        {
            GpuUsage.Clear();
            hardwareInfo.RefreshVideoControllerList();

            if (OperatingSystem.IsWindows())
            {
                UpdateGpuInfoForWindows();
            }
            else if (OperatingSystem.IsLinux())
            {
                UpdateGpuInfoForLinux();
            }

            if (GpuUsage.Count == 0)
            {
                var gpuName = hardwareInfo.VideoControllerList.FirstOrDefault()?.Name ?? "GPU";
                GpuUsage[gpuName] = "N/A";
            }
        }
        catch
        {
            GpuUsage.Clear();
            var gpuName = hardwareInfo.VideoControllerList.FirstOrDefault()?.Name ?? "GPU";
            GpuUsage[gpuName] = "N/A";
        }
    }

    [SupportedOSPlatform("windows")]
    private void UpdateGpuInfoForWindows()
    {
        if (TryReadNvidiaGpuUsage())
        {
            return;
        }

        var gpuName = hardwareInfo.VideoControllerList.FirstOrDefault()?.Name ?? "GPU";

        try
        {
            if (windowsGpuCounters is null)
            {
                if (PerformanceCounterCategory.Exists("GPU Engine"))
                {
                    windowsGpuCounters = new List<PerformanceCounter>();
                    var category = new PerformanceCounterCategory("GPU Engine");
                    var instances = category.GetInstanceNames()
                        .Where(name => name.Contains("engtype_3D", StringComparison.OrdinalIgnoreCase));

                    foreach (var instance in instances)
                    {
                        var counter = new PerformanceCounter("GPU Engine", "Utilization Percentage", instance, true);
                        try
                        {
                            _ = counter.NextValue();
                            windowsGpuCounters.Add(counter);
                        }
                        catch
                        {
                            counter.Dispose();
                        }
                    }
                }
            }

            if (windowsGpuCounters is null || windowsGpuCounters.Count == 0)
            {
                GpuUsage[gpuName] = "N/A";
                return;
            }

            var totalUsage = windowsGpuCounters.Sum(counter =>
            {
                try
                {
                    return Math.Max(0f, counter.NextValue());
                }
                catch
                {
                    return 0f;
                }
            });

            totalUsage = Math.Clamp(totalUsage, 0f, 100f);
            GpuUsage[gpuName] = $"{totalUsage:0.#}%";
        }
        catch
        {
            GpuUsage[gpuName] = "N/A";
        }
    }

    private void UpdateGpuInfoForLinux()
    {
        if (TryReadNvidiaGpuUsage())
        {
            return;
        }

        if (TryReadLinuxSysfsGpuUsage())
        {
            return;
        }

        var gpuName = hardwareInfo.VideoControllerList.FirstOrDefault()?.Name ?? "GPU";
        GpuUsage[gpuName] = "N/A";
    }

    private bool TryReadNvidiaGpuUsage()
    {
        var output = RunProcessAndReadStandardOutput(
            "nvidia-smi",
            "--query-gpu=name,utilization.gpu --format=csv,noheader,nounits");

        if (string.IsNullOrWhiteSpace(output))
        {
            return false;
        }

        var found = false;
        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split(',', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                continue;
            }

            var name = string.IsNullOrWhiteSpace(parts[0]) ? "NVIDIA GPU" : parts[0];
            if (!int.TryParse(parts[1], out var usagePercent))
            {
                continue;
            }

            GpuUsage[name] = $"{Math.Clamp(usagePercent, 0, 100)}%";
            found = true;
        }

        return found;
    }

    private bool TryReadLinuxSysfsGpuUsage()
    {
        const string drmPath = "/sys/class/drm";
        if (!Directory.Exists(drmPath))
        {
            return false;
        }

        var found = false;
        foreach (var cardPath in Directory.GetDirectories(drmPath, "card*"))
        {
            var cardName = Path.GetFileName(cardPath);
            if (cardName.Contains('-', StringComparison.Ordinal))
            {
                continue;
            }

            var usagePercent = ReadLinuxSysfsGpuBusyPercent(cardPath);
            if (usagePercent is null)
            {
                continue;
            }

            var name = ReadLinuxGpuName(cardPath) ?? cardName;
            GpuUsage[name] = $"{usagePercent.Value:0.#}%";
            found = true;
        }

        return found;
    }

    private static double? ReadLinuxSysfsGpuBusyPercent(string cardPath)
    {
        var candidates = new[]
        {
            Path.Combine(cardPath, "device", "gpu_busy_percent"),
            Path.Combine(cardPath, "gt_busy_percent"),
            Path.Combine(cardPath, "gt", "gt0", "busy_percent")
        };

        foreach (var path in candidates)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            var raw = File.ReadAllText(path).Trim();
            if (double.TryParse(raw, out var usagePercent))
            {
                return Math.Clamp(usagePercent, 0d, 100d);
            }
        }

        return null;
    }

    private static string? ReadLinuxGpuName(string cardPath)
    {
        var ueventPath = Path.Combine(cardPath, "device", "uevent");
        if (!File.Exists(ueventPath))
        {
            return null;
        }

        foreach (var line in File.ReadLines(ueventPath))
        {
            if (line.StartsWith("DRIVER=", StringComparison.Ordinal))
            {
                return line[7..];
            }
        }

        return null;
    }

    private static string? RunProcessAndReadStandardOutput(string fileName, string arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(2000);

            return process.ExitCode == 0 ? output : null;
        }
        catch
        {
            return null;
        }
    }
}
