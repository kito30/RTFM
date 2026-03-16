using Microsoft.AspNetCore.SignalR;

namespace backdoor.services;

public class PulseWorker : BackgroundService
{
    private readonly ISysMonitor monitor; 
    

    // This is to use the hub outside of the hub class, so we can send data to clients from this background service
    private readonly IHubContext<MonitorHub> hubContext;

    public PulseWorker(ISysMonitor monitor, IHubContext<MonitorHub> hubContext)
    {
        this.monitor = monitor;
        this.hubContext = hubContext;
    }
    
    // This method will run in the background and send system info to connected clients every second
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            monitor.UpdateSystemInfo();
            var payload = new
            {
                cpuUsage = monitor.CpuUsage,
                memoryUsage = monitor.MemoryUsage,
                diskUsage = new Dictionary<string, string>(monitor.DiskUsage),
                gpuUsage = new Dictionary<string, string>(monitor.GpuUsage),
                os = monitor.OS
            };
            await hubContext.Clients.All.SendAsync(
                "ReceiveData",
                payload,
                cancellationToken: stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }
}