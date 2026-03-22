using Hardware.Info;

namespace backdoor.services;

public partial class SysMonitor
{
    private void UpdateCpuInfo()
    {
        try
        {
            hardwareInfo.RefreshCPUList();

            var cpu = hardwareInfo.CpuList.FirstOrDefault();
            if (cpu is null)
            {
                CpuUsage = "N/A";
                return;
            }

            var cpuPercent = cpu.CpuCoreList.Count > 0
                ? cpu.CpuCoreList.Average(core => (double)core.PercentProcessorTime)
                : cpu.PercentProcessorTime;

            CpuUsage = $"{cpuPercent:0.#}%";
        }
        catch
        {
            CpuUsage = "N/A";
        }
    }
}
