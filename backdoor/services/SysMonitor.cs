
using Hardware.Info;

namespace backdoor.services;
public class  SysMonitor: ISysMonitor
{
    public string CpuUsage { get; private set; }
    public string GpuUsage { get; private set; }
    public string MemoryUsage { get; private set; }
    public string DiskUsage { get; private set; }
    public string OS { get; private set; }

    private readonly IHardwareInfo hardwareInfo;
    public SysMonitor(IHardwareInfo hardwareInfo)
    {
        CpuUsage = "";
        GpuUsage = "";
        MemoryUsage = "";
        DiskUsage = "";
        OS = GetOperatingSystem();
        this.hardwareInfo = hardwareInfo;
    }

    private static string GetOperatingSystem()
    {
        return Environment.OSVersion.ToString();
    }
    public void UpdateSystemInfo()
    {
        try
        {
            hardwareInfo.RefreshCPUList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error refreshing CPU info: {ex.Message}");
            CpuUsage = "N/A";
        }
        var cpu = hardwareInfo.CpuList[0]; // Get the first CPU

        Console.WriteLine($"CPU Name: {cpu.Name}");

        var cpu0 = hardwareInfo.CpuList[0].CpuCoreList[0].PercentProcessorTime;
        Console.WriteLine($"CPU Usage: {cpu0}%");
    }
}