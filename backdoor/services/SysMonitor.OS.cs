namespace backdoor.services;

public partial class SysMonitor
{
    private void UpdateOperatingSystemInfo()
    {
        try
        {
            hardwareInfo.RefreshOperatingSystem();
            OS = hardwareInfo.OperatingSystem?.Name ?? OS;
        }
        catch
        {
            // Keep last known OS value.
        }
    }
}
