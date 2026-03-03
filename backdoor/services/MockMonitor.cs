namespace backdoor.services;


public class MockMonitor : ISysMonitor
{
    public double GetCpuUsage() => Random.Shared.NextDouble() * 100;
    public double GetGpuUsage() => Random.Shared.NextDouble() * 100;
    public double GetMemoryUsage() => Random.Shared.NextDouble() * 100;
    public double GetDiskUsage() => Random.Shared.NextDouble() * 100;
    
}