using Hardware.Info;

namespace backdoor.services;

public partial class SysMonitor : ISysMonitor
{
    public string CpuUsage { get; private set; }
    public Dictionary<string, string> GpuUsage { get; private set; }
    public string MemoryUsage { get; private set; }
    public Dictionary<string, string> DiskUsage { get; private set; }
    public string OS { get; private set; }

    private readonly IHardwareInfo hardwareInfo;
    private Dictionary<string, System.Diagnostics.PerformanceCounter>? diskCountersByName;
    private List<System.Diagnostics.PerformanceCounter>? windowsGpuCounters;
    private long? linuxTotalIoTimeMs;
    private DateTimeOffset? linuxDiskSampleTimestamp;

    public SysMonitor(IHardwareInfo hardwareInfo)
    {
        CpuUsage = "0%";
        GpuUsage = new Dictionary<string, string>();
        MemoryUsage = "0%";
        DiskUsage = new Dictionary<string, string>();
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
