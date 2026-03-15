using Hardware.Info;

namespace backdoor.services;

public partial class SysMonitor : ISysMonitor
{
    public string CpuUsage { get; private set; }
    public string GpuUsage { get; private set; }
    public string MemoryUsage { get; private set; }
    public IReadOnlyList<DiskMetric> DiskUsage { get; private set; }
    public string OS { get; private set; }

    private readonly IHardwareInfo hardwareInfo;
    private Dictionary<string, System.Diagnostics.PerformanceCounter>? diskCountersByName;
    private long? linuxTotalIoTimeMs;
    private DateTimeOffset? linuxDiskSampleTimestamp;

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
}
