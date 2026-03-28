using Xunit;
using Philiprehberger.Outbox;

namespace Philiprehberger.Outbox.Tests;

public class OutboxDiagnosticsTests : IDisposable
{
    public OutboxDiagnosticsTests()
    {
        OutboxDiagnostics.Reset();
    }

    public void Dispose()
    {
        OutboxDiagnostics.Reset();
    }

    [Fact]
    public void MessageEnqueued_RaisesEvent()
    {
        OutboxMessage? received = null;
        OutboxDiagnostics.MessageEnqueued += m => received = m;

        var message = new OutboxMessage(Guid.NewGuid(), "Test", "{}", DateTimeOffset.UtcNow);
        OutboxDiagnostics.OnMessageEnqueued(message);

        Assert.NotNull(received);
        Assert.Equal(message.Id, received!.Id);
    }

    [Fact]
    public void MessageDispatched_RaisesEvent()
    {
        OutboxMessage? received = null;
        OutboxDiagnostics.MessageDispatched += m => received = m;

        var message = new OutboxMessage(Guid.NewGuid(), "Test", "{}", DateTimeOffset.UtcNow);
        OutboxDiagnostics.OnMessageDispatched(message);

        Assert.NotNull(received);
        Assert.Equal(message.Id, received!.Id);
    }

    [Fact]
    public void MessageFailed_RaisesEvent()
    {
        OutboxMessage? received = null;
        OutboxDiagnostics.MessageFailed += m => received = m;

        var message = new OutboxMessage(Guid.NewGuid(), "Test", "{}", DateTimeOffset.UtcNow);
        OutboxDiagnostics.OnMessageFailed(message);

        Assert.NotNull(received);
        Assert.Equal(message.Id, received!.Id);
    }

    [Fact]
    public void MessageDeadLettered_RaisesEvent()
    {
        OutboxMessage? received = null;
        OutboxDiagnostics.MessageDeadLettered += m => received = m;

        var message = new OutboxMessage(Guid.NewGuid(), "Test", "{}", DateTimeOffset.UtcNow);
        OutboxDiagnostics.OnMessageDeadLettered(message);

        Assert.NotNull(received);
        Assert.Equal(message.Id, received!.Id);
    }

    [Fact]
    public void Reset_ClearsAllSubscribers()
    {
        var called = false;
        OutboxDiagnostics.MessageEnqueued += _ => called = true;

        OutboxDiagnostics.Reset();
        OutboxDiagnostics.OnMessageEnqueued(new OutboxMessage(Guid.NewGuid(), "Test", "{}", DateTimeOffset.UtcNow));

        Assert.False(called);
    }
}
