using System.Diagnostics;
using System.Runtime.Versioning;

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
                GpuUsage.Add(new GpuInfo(gpuName, "N/A"));
            }
        }
        catch
        {
            GpuUsage.Clear();
            var gpuName = hardwareInfo.VideoControllerList.FirstOrDefault()?.Name ?? "GPU";
            GpuUsage.Add(new GpuInfo(gpuName, "N/A"));
        }
    }

    [SupportedOSPlatform("windows")]
    private void UpdateGpuInfoForWindows()
    {
        // 1. Try NVIDIA-SMI first (User says this works fine)
        TryReadNvidiaGpuUsage();

        try
        {
            if (!PerformanceCounterCategory.Exists("GPU Engine")) return;

            // Simple one-time initialization of counters
            if (gpuCountersByGpuId == null)
            {
                gpuCountersByGpuId = new Dictionary<string, List<PerformanceCounter>>();
                var category = new PerformanceCounterCategory("GPU Engine");
                var instances = category.GetInstanceNames()
                    .Where(n => n.Contains("engtype_3D", StringComparison.OrdinalIgnoreCase));

                foreach (var instance in instances)
                {
                    // Extract physical index (phys_0, phys_1...) which usually matches hardwareInfo order
                    string physId = instance.Contains("phys_") ? instance.Split("phys_")[1].Split('_')[0] : "0";

                    if (!gpuCountersByGpuId.ContainsKey(physId))
                        gpuCountersByGpuId[physId] = new List<PerformanceCounter>();

                    var counter = new PerformanceCounter("GPU Engine", "Utilization Percentage", instance, true);
                    try { _ = counter.NextValue(); gpuCountersByGpuId[physId].Add(counter); } catch { counter.Dispose(); }
                }
            }

            // Basic reading from the cached counters
            foreach (var gpuEntry in gpuCountersByGpuId)
            {
                float maxUsage = 0;
                foreach (var counter in gpuEntry.Value)
                {
                    try { maxUsage = Math.Max(maxUsage, counter.NextValue()); } catch { }
                }

                int.TryParse(gpuEntry.Key, out int idx);
                var controller = hardwareInfo.VideoControllerList.ElementAtOrDefault(idx);
                string name = controller?.Name ?? $"GPU {gpuEntry.Key}";

                // Don't duplicate if nvidia-smi already caught it
                if (!GpuUsage.Any(g => g.Name == name))
                {
                    GpuUsage.Add(new GpuInfo(name, $"{Math.Clamp(maxUsage, 0f, 100f):0.#}%"));
                }
            }
        }
        catch { }
    }

    private void UpdateGpuInfoForLinux()
    {
        TryReadNvidiaGpuUsage();
        TryReadLinuxSysfsGpuUsage();
    }

    private void TryReadNvidiaGpuUsage()
    {
        var output = RunProcessAndReadStandardOutput(
            "nvidia-smi",
            "--query-gpu=name,utilization.gpu --format=csv,noheader,nounits");

        if (string.IsNullOrWhiteSpace(output)) return;

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = line.Split(',', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2) continue;

            var name = string.IsNullOrWhiteSpace(parts[0]) ? "NVIDIA GPU" : parts[0];
            if (int.TryParse(parts[1], out var usagePercent))
            {
                GpuUsage.Add(new GpuInfo(name, $"{Math.Clamp(usagePercent, 0, 100)}%"));
            }
        }
    }

    private void TryReadLinuxSysfsGpuUsage()
    {
        const string drmPath = "/sys/class/drm";
        if (!Directory.Exists(drmPath)) return;

        foreach (var cardPath in Directory.GetDirectories(drmPath, "card*"))
        {
            var cardName = Path.GetFileName(cardPath);
            if (cardName.Contains('-')) continue;

            var usagePercent = ReadLinuxSysfsGpuBusyPercent(cardPath);
            if (usagePercent == null) continue;

            var name = ReadLinuxGpuName(cardPath) ?? cardName;
            if (!GpuUsage.Any(g => g.Name == name))
            {
                GpuUsage.Add(new GpuInfo(name, $"{usagePercent.Value:0.#}%"));
            }
        }
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
            if (File.Exists(path))
            {
                var raw = File.ReadAllText(path).Trim();
                if (double.TryParse(raw, out var usage)) return Math.Clamp(usage, 0d, 100d);
            }
        }
        return null;
    }

    private static string? ReadLinuxGpuName(string cardPath)
    {
        var ueventPath = Path.Combine(cardPath, "device", "uevent");
        if (!File.Exists(ueventPath)) return null;

        foreach (var line in File.ReadLines(ueventPath))
        {
            if (line.StartsWith("DRIVER=")) return line[7..];
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
        catch { return null; }
    }
}
