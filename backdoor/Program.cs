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
builder.Services.AddSingleton<EmailAlarm>();
builder.Services.AddSingleton<AlertSettingsStore>();
///register a background service
builder.Services.AddHostedService<PulseWorker>();


var app = builder.Build();

app.UseCors("AllowAll");
app.MapHub<MonitorHub>("/hubs/monitor");

app.MapGet("/api/alerts/settings", (AlertSettingsStore store) =>
{
    var current = store.GetCurrentAlertSettings();
    return Results.Ok(AlertSettingsStore.ToResponse(current));
});

// store is from dependency injection, it will resolve the singleton instance of AlertSettingsStore that we registered in the services
// The request will be automatically deserialized from json to AlertSettingsUpdateRequest by the framework
app.MapPost("/api/alerts/settings", (AlertSettingsUpdateRequest request, AlertSettingsStore store) =>
{
    var updated = store.UpdateAlertSettings(request);
    return Results.Ok(AlertSettingsStore.ToPostResponse(updated));
});

app.Run();
