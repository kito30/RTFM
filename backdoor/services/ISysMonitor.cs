namespace backdoor.services;

public interface ISysMonitor
{
    string CpuUsage { get; }
    string GpuUsage { get; }
    string MemoryUsage { get; }
    string DiskUsage { get; }
    string OS { get; }
    void UpdateSystemInfo();
}

