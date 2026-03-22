
namespace backdoor.services;


/// <summary>
///  Class that hold the current setting threshold exsisted in backend idk
/// </summary>
public sealed class AlertSettingsCurrent
{
    public double CpuThresholdPercent { get; init; }
    public double MemoryThresholdPercent { get; init; }
    public double GpuThresholdPercent { get; init; }
    public double DiskThresholdPercent { get; init; }
    public int CooldownMinutes { get; init; }
    public string AlertToEmail { get; init; } = string.Empty;
}

/// <summary>
/// Class that hold the update setting from the front end send to backend which kinda prevent race condition.
/// </summary>
public sealed class AlertSettingsUpdateRequest
{
    public double? CpuThresholdPercent { get; init; }
    public double? MemoryThresholdPercent { get; init; }
    public double? GpuThresholdPercent { get; init; }
    public double? DiskThresholdPercent { get; init; }
    public int? CooldownMinutes { get; init; }
    public string? AlertToEmail { get; init; }
}

public sealed class AlertSettingsResponse
{
    public double CpuThresholdPercent { get; init; }
    public double MemoryThresholdPercent { get; init; }
    public double GpuThresholdPercent { get; init; }
    public double DiskThresholdPercent { get; init; }
    public int CooldownMinutes { get; init; }
}

public sealed class AlertSettingsPostResponse
{
    public double CpuThresholdPercent { get; init; }
    public double MemoryThresholdPercent { get; init; }
    public double GpuThresholdPercent { get; init; }
    public double DiskThresholdPercent { get; init; }
    public int CooldownMinutes { get; init; }
    public string? AlertToEmail { get; init; }
}

public sealed class AlertSettingsStore
{
    private readonly object gate = new();
    private AlertSettingsCurrent current;

    // Initialize the AlertSettingsStore with default values from configuration or fallback defaults.
    public AlertSettingsStore(IConfiguration configuration)
    {
        current = new AlertSettingsCurrent
        {
            CpuThresholdPercent = ReadPercent(configuration, "Alert:CpuThresholdPercent", 90d),
            MemoryThresholdPercent = ReadPercent(configuration, "Alert:MemoryThresholdPercent", 90d),
            GpuThresholdPercent = ReadPercent(configuration, "Alert:GpuThresholdPercent", 95d),
            DiskThresholdPercent = ReadPercent(configuration, "Alert:DiskThresholdPercent", 95d),
            CooldownMinutes = Math.Max(configuration.GetValue("Alert:CooldownMinutes", 10), 1),
            AlertToEmail = configuration["Gmail:AlertTo"] ?? configuration["Gmail:UserEmail"] ?? string.Empty
        };
    }

    // Read the current AlrtSettingsCurrent and lock it.
    public  AlertSettingsCurrent GetCurrentAlertSettings()
    {
        lock (gate)
        {
            return current;
        }
    }

    // Update the current AlertSettingsCurrent with the new value from the front end and lock it.
    public AlertSettingsCurrent UpdateAlertSettings(AlertSettingsUpdateRequest request)
    {
        /// Automactically check, pretty cool
        lock (gate)
        {
            current = new AlertSettingsCurrent
            {
                CpuThresholdPercent = ResolvePercent(request.CpuThresholdPercent, current.CpuThresholdPercent),
                MemoryThresholdPercent = ResolvePercent(request.MemoryThresholdPercent, current.MemoryThresholdPercent),
                GpuThresholdPercent = ResolvePercent(request.GpuThresholdPercent, current.GpuThresholdPercent),
                DiskThresholdPercent = ResolvePercent(request.DiskThresholdPercent, current.DiskThresholdPercent),
                CooldownMinutes = request.CooldownMinutes is null ? current.CooldownMinutes : Math.Max(request.CooldownMinutes.Value, 1),
                AlertToEmail = request.AlertToEmail?.Trim() ?? current.AlertToEmail
            };

            return current;
        }
    }

    private static double ResolvePercent(double? value, double fallback)
    {
        if (value is null)
        {
            return fallback;
        }

        if (value <= 0 || value > 100)
        {
            return fallback;
        }

        return value.Value;
    }

    private static double ReadPercent(IConfiguration configuration, string key, double fallback)
    {
        var value = configuration.GetValue<double?>(key);
        if (value is null || value <= 0 || value > 100)
        {
            return fallback;
        }

        return value.Value;
    }

    public static AlertSettingsResponse ToResponse(AlertSettingsCurrent current)
    {
        return new AlertSettingsResponse
        {
            CpuThresholdPercent = current.CpuThresholdPercent,
            MemoryThresholdPercent = current.MemoryThresholdPercent,
            GpuThresholdPercent = current.GpuThresholdPercent,
            DiskThresholdPercent = current.DiskThresholdPercent,
            CooldownMinutes = current.CooldownMinutes,
        };
    }

    public static AlertSettingsPostResponse ToPostResponse(AlertSettingsCurrent current)
    {
        return new AlertSettingsPostResponse
        {
            CpuThresholdPercent = current.CpuThresholdPercent,
            MemoryThresholdPercent = current.MemoryThresholdPercent,
            GpuThresholdPercent = current.GpuThresholdPercent,
            DiskThresholdPercent = current.DiskThresholdPercent,
            CooldownMinutes = current.CooldownMinutes,
            AlertToEmail = string.IsNullOrWhiteSpace(current.AlertToEmail) ? null : current.AlertToEmail,
        };
    }
}