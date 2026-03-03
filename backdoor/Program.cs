using backdoor.services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<ISysMonitor, MockMonitor>();
builder.Services.AddHostedService<PulseWorker>();
var app = builder.Build();




app.MapHub<MonitorHub>("/hubs/monitor");
app.Run();
