namespace backdoor.services;

public interface ISysMonitor
{
    string CpuUsage { get; }
    Dictionary<string, string> GpuUsage { get; }
    string MemoryUsage { get; }
    Dictionary<string, string> DiskUsage { get; }
    string OS { get; }
    void UpdateSystemInfo();
}

