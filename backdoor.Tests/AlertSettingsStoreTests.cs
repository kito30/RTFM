using backdoor.services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace backdoor.Tests;

public sealed class AlertSettingsStoreTests
{
    [Fact]
    public void UsesDefaults_WhenInputIsInvalid()
    {
        var configuration = BuildConfig(new Dictionary<string, string?>
        {
            ["Alert:CpuThresholdPercent"] = "0",
            ["Alert:MemoryThresholdPercent"] = "101",
            ["Alert:GpuThresholdPercent"] = "-1",
            ["Alert:DiskThresholdPercent"] = "-1",
            ["Alert:CooldownMinutes"] = "0",
            ["Gmail:UserEmail"] = "ops@example.com"
        });

        var store = new AlertSettingsStore(configuration);
        var current = store.GetCurrentAlertSettings();

        Assert.Equal(90d, current.CpuThresholdPercent);
        Assert.Equal(90d, current.MemoryThresholdPercent);
        Assert.Equal(95d, current.GpuThresholdPercent);
        Assert.Equal(95d, current.DiskThresholdPercent);
        Assert.Equal(1, current.CooldownMinutes);
        Assert.Equal("ops@example.com", current.AlertToEmail);
    }

    [Fact]
    public void Update_ChangesOnlyProvidedValues()
    {
        var configuration = BuildConfig(new Dictionary<string, string?>
        {
            ["Alert:CpuThresholdPercent"] = "80",
            ["Alert:MemoryThresholdPercent"] = "85",
            ["Alert:GpuThresholdPercent"] = "90",
            ["Alert:DiskThresholdPercent"] = "92",
            ["Alert:CooldownMinutes"] = "15",
            ["Gmail:AlertTo"] = "old@example.com"
        });

        var store = new AlertSettingsStore(configuration);

        var updated = store.UpdateAlertSettings(new AlertSettingsUpdateRequest
        {
            CpuThresholdPercent = 70,
            MemoryThresholdPercent = null,
            GpuThresholdPercent = 88,
            DiskThresholdPercent = 93,
            CooldownMinutes = 20,
            AlertToEmail = "  new@example.com  "
        });

        Assert.Equal(70d, updated.CpuThresholdPercent);
        Assert.Equal(85d, updated.MemoryThresholdPercent);
        Assert.Equal(88d, updated.GpuThresholdPercent);
        Assert.Equal(93d, updated.DiskThresholdPercent);
        Assert.Equal(20, updated.CooldownMinutes);
        Assert.Equal("new@example.com", updated.AlertToEmail);
    }

    
    // Add config with variable instead of conf file for testing
    private static IConfiguration BuildConfig(IDictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
