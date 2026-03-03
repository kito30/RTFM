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
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var cpu = monitor.GetCpuUsage();
            await hubContext.Clients.All.SendAsync("ReceiveData", cpu, cancellationToken: stoppingToken);
            Console.WriteLine($"[RTFM] Sent Pulse: {cpu}%");
            await Task.Delay(1000, stoppingToken); // Adjust the delay as needed
        }
    }
}