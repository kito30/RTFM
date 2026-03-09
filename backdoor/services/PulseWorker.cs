using Microsoft.AspNetCore.SignalR;

namespace backdoor.services;

public class PulseWorker : BackgroundService
{
    private readonly ISysMonitor monitor; 
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
            var cpu = monitor.GetCpuUsage();
            var memory = monitor.GetMemoryUsage();
            var disk = monitor.GetDiskUsage();
            var gpu = monitor.GetGpuUsage();
            await hubContext.Clients.All.SendAsync("ReceiveData", cpu, memory, disk, gpu, cancellationToken: stoppingToken);
            Console.WriteLine($"[RTFM] Sent Pulse: {cpu}%");
            await Task.Delay(1000, stoppingToken); // Adjust the delay as needed
        }
    }
}