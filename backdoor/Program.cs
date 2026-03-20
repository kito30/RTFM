using backdoor.interfaces;
using backdoor.services;
using Hardware.Info;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});
builder.Services.AddSingleton<ISysMonitor, SysMonitor>();
builder.Services.AddSingleton<IHardwareInfo, HardwareInfo>();
///register a background service
builder.Services.AddHostedService<PulseWorker>();

var app = builder.Build();

app.UseCors("AllowAll");

app.MapHub<MonitorHub>("/hubs/monitor");
app.Run();
