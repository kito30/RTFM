using Microsoft.AspNetCore.SignalR;

namespace backdoor.services;

public class PulseWorker : BackgroundService
{
    private readonly ISysMonitor monitor; 
    private readonly IHubContext<MonitorHub> hubContext;
}