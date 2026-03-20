namespace backdoor.interfaces;

public interface ISysMonitor
{
    string CpuUsage { get; }
    List<GpuInfo> GpuUsage { get; }
    string MemoryUsage { get; }
    Dictionary<string, string> DiskUsage { get; }
    string OS { get; }
    void UpdateSystemInfo();
}
