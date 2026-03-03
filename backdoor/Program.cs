using backdoor.services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISysMonitor, MockMonitor>();
var app = builder.Build();
app.MapGet("/", (ISysMonitor monitor) => $"CPU is at: {monitor.GetCpuUsage()}%");

app.Run();
