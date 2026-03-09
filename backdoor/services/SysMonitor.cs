namespace backdoor.services;
public class  SysMonitor: ISysMonitor
{
    public double CpuUsage { get; private set; }
    public double GpuUsage { get; private set; }
    public double MemoryUsage { get; private set; }
    public double DiskUsage { get; private set; }
    public string OS { get; private set; }

    public SysMonitor()
    {
        CpuUsage = 0;
        GpuUsage = 0;
        MemoryUsage = 0;
        DiskUsage = 0;
        OS = GetOperatingSystem();
    }

    private static string GetOperatingSystem()
    {
        return Environment.OSVersion.ToString();
    }
    public void UpdateSystemInfo()
    {
    }
}