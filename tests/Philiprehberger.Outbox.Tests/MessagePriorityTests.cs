using Xunit;
using Philiprehberger.Outbox;

namespace Philiprehberger.Outbox.Tests;

public class MessagePriorityTests
{
    [Fact]
    public void Priority_DefaultIsNormal()
    {
        var message = new OutboxMessage(Guid.NewGuid(), "Test", "{}", DateTimeOffset.UtcNow);

        Assert.Equal(MessagePriority.Normal, message.Priority);
    }

    [Fact]
    public void Priority_CriticalHasHighestValue()
    {
        Assert.True(MessagePriority.Critical > MessagePriority.High);
        Assert.True(MessagePriority.High > MessagePriority.Normal);
        Assert.True(MessagePriority.Normal > MessagePriority.Low);
    }

    [Fact]
    public void Priority_CanBeSetExplicitly()
    {
        var message = new OutboxMessage(
            Guid.NewGuid(), "Test", "{}", DateTimeOffset.UtcNow,
            Priority: MessagePriority.Critical);

        Assert.Equal(MessagePriority.Critical, message.Priority);
    }
}
