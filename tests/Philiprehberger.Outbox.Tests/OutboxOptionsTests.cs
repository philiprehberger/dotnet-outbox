using Xunit;
using Philiprehberger.Outbox;

namespace Philiprehberger.Outbox.Tests;

public class OutboxOptionsTests
{
    [Fact]
    public void Defaults_BatchSizeIs100()
    {
        var options = new OutboxOptions();

        Assert.Equal(100, options.BatchSize);
    }

    [Fact]
    public void Defaults_MaxRetriesIs3()
    {
        var options = new OutboxOptions();

        Assert.Equal(3, options.MaxRetries);
    }

    [Fact]
    public void EffectivePollingInterval_WhenNull_DefaultsToFiveSeconds()
    {
        var options = new OutboxOptions();

        Assert.Equal(TimeSpan.FromSeconds(5), options.EffectivePollingInterval);
    }

    [Fact]
    public void EffectivePollingInterval_WhenSet_ReturnsConfiguredValue()
    {
        var interval = TimeSpan.FromSeconds(30);
        var options = new OutboxOptions(PollingInterval: interval);

        Assert.Equal(interval, options.EffectivePollingInterval);
    }
}
