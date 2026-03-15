namespace backdoor.services;

public partial class SysMonitor
{
    private void UpdateMemoryInfo()
    {
        try
        {
            hardwareInfo.RefreshMemoryStatus();

            var totalMemory = hardwareInfo.MemoryStatus.TotalPhysical;
            if (totalMemory == 0)
            {
                MemoryUsage = "N/A";
                return;
            }

            var availableMemory = Math.Min(hardwareInfo.MemoryStatus.AvailablePhysical, totalMemory);
            var usedMemory = totalMemory - availableMemory;
            MemoryUsage = $"{(double)usedMemory / totalMemory * 100:0.#}%";
        }
        catch
        {
            MemoryUsage = "N/A";
        }
    }
}
