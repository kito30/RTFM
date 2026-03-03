namespace backdoor.services;

public interface ISysMonitor
{
    double GetCpuUsage();
    double GetGpuUsage();
    double GetMemoryUsage();
    double GetDiskUsage();
}

