namespace backdoor.services;

public interface ISysMonitor
{
    double CpuUsage { get; }
    double GpuUsage { get; }
    double MemoryUsage { get; }
    double DiskUsage { get; }
    string OS { get; }
}

