using Microsoft.AspNetCore.SignalR;
using System.Globalization;
using System.Text.RegularExpressions;

namespace backdoor.services;

public class PulseWorker : BackgroundService
{
    private readonly ISysMonitor monitor;
    private readonly EmailAlarm emailAlarm;
    private readonly IConfiguration configuration;
    private readonly ILogger<PulseWorker> logger;
    private readonly AlertSettingsStore alertSettingsStore;

    // This is to use the hub outside of the hub class, so we can send data to clients from this background service
    private readonly IHubContext<MonitorHub> hubContext;

    private readonly Dictionary<string, DateTimeOffset> lastAlertSentAt = new();
    private readonly Dictionary<string, bool> isMetricBreached = new();

    public PulseWorker(
        ISysMonitor monitor,
        IHubContext<MonitorHub> hubContext,
        EmailAlarm emailAlarm,
        IConfiguration configuration,
        ILogger<PulseWorker> logger,
        AlertSettingsStore alertSettingsStore)
    {
        this.monitor = monitor;
        this.hubContext = hubContext;
        this.emailAlarm = emailAlarm;
        this.configuration = configuration;
        this.logger = logger;
        this.alertSettingsStore = alertSettingsStore;
    }
    
    // This method will run in the background and send system info to connected clients every second
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                monitor.UpdateSystemInfo();
                var payload = new
                {
                    cpuUsage = monitor.CpuUsage,
                    memoryUsage = monitor.MemoryUsage,
                    diskUsage = monitor.DiskUsage,
                    gpuUsage = monitor.GpuUsage,
                    os = monitor.OS
                };
                await hubContext.Clients.All.SendAsync(
                    "ReceiveData",
                    payload,
                    cancellationToken: stoppingToken);

                await CheckAndSendAlerts(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Error while processing monitor pulse");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task CheckAndSendAlerts(CancellationToken stoppingToken)
    {
        var settings = alertSettingsStore.GetCurrentAlertSettings();

        //Get minute duration of cooldown
        var cooldown = TimeSpan.FromMinutes(settings.CooldownMinutes);

        await CheckMetricAlert("CPU", monitor.CpuUsage, settings.CpuThresholdPercent, cooldown, stoppingToken);
        await CheckMetricAlert("Memory", monitor.MemoryUsage, settings.MemoryThresholdPercent, cooldown, stoppingToken);

        foreach (var gpu in monitor.GpuUsage)
        {
            await CheckMetricAlert($"GPU:{gpu.Name}", gpu.Usage, settings.GpuThresholdPercent, cooldown, stoppingToken);
        }

        foreach (var disk in monitor.DiskUsage)
        {
            await CheckMetricAlert($"Disk:{disk.Key}", disk.Value, settings.DiskThresholdPercent, cooldown, stoppingToken);
        }
    }

    private async Task CheckMetricAlert(string metricName, string usageText, double thresholdPercent, TimeSpan cooldown, CancellationToken stoppingToken)
    {
        if (!TryParseUsagePercent(usageText, out var usagePercent))
        {
            return;
        }

        var currentlyBreached = usagePercent >= thresholdPercent;
        if (!currentlyBreached)
        {
            return;
        }
        // Check if the metric was already breached
        var wasBreached = isMetricBreached.TryGetValue(metricName, out var oldValue) && oldValue;

        // Update the current breached state for this metric
        isMetricBreached[metricName] = currentlyBreached;

        // only send the alert if it was not breached before or if the cooldown time has passed since the last alert for this metric
        if (!ShouldSend(metricName, wasBreached, cooldown))
        {
            return;
        }

        lastAlertSentAt[metricName] = DateTimeOffset.UtcNow;
        var alertMessage = $"{metricName} reached {usagePercent:0.#}% (threshold {thresholdPercent:0.#}%)";

        await hubContext.Clients.All.SendAsync(
            "SendAlert",
            new
            {
                metric = metricName,
                usage = usagePercent,
                threshold = thresholdPercent,
                message = alertMessage,
                timestamp = DateTimeOffset.UtcNow
            },
            cancellationToken: stoppingToken);

        var toEmail = configuration["Gmail:AlertTo"] ?? configuration["Gmail:UserEmail"];
        if (!string.IsNullOrWhiteSpace(toEmail))
        {
            await emailAlarm.SendAsyncEmail(
                toEmail,
                $"[ALERT] {metricName} usage is high",
                $"{alertMessage}\nTime (UTC): {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}");
        }

        logger.LogWarning("{AlertMessage}", alertMessage);
    }

    // check for each metric if we should send an alert based on if it was breached before and the cooldown time    
    private bool ShouldSend(string metricName, bool wasBreached, TimeSpan cooldown)
    {
        if (!wasBreached)
        {
            return true;
        }

        if (!lastAlertSentAt.TryGetValue(metricName, out var lastSent))
        {
            return true;
        }

        return DateTimeOffset.UtcNow - lastSent >= cooldown;
    }

    //Parase a string like "85%" or "85.5%" and return the numeric value, if parsing fails return false
    private static bool TryParseUsagePercent(string rawValue, out double value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        var match = Regex.Match(rawValue, @"\d+(\.\d+)?", RegexOptions.CultureInvariant);
        if (!match.Success)
        {
            return false;
        }

        return double.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

}