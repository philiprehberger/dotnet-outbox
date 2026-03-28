using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Philiprehberger.Outbox;

namespace Philiprehberger.Outbox.Tests;

public class OutboxRelayServiceTests : IDisposable
{
    public OutboxRelayServiceTests()
    {
        OutboxDiagnostics.Reset();
    }

    public void Dispose()
    {
        OutboxDiagnostics.Reset();
    }

    [Fact]
    public async Task ProcessAsync_ExceededRetries_MovesToDeadLetterQueue()
    {
        var store = new FakeOutboxStore();
        var dlq = new DeadLetterInMemoryStore();
        var dispatcher = new FakeDispatcher();
        var options = new OutboxOptions(MaxRetries: 3);
        var service = new OutboxRelayService(store, dispatcher, dlq, options, NullLogger<OutboxRelayService>.Instance);

        var message = new OutboxMessage(Guid.NewGuid(), "Test", "{}", DateTimeOffset.UtcNow, RetryCount: 3);
        store.Messages.Add(message);

        await service.ProcessAsync(CancellationToken.None);

        var deadLettered = await dlq.GetAllAsync();
        Assert.Single(deadLettered);
        Assert.Equal(message.Id, deadLettered[0].Id);
        Assert.False(dispatcher.DispatchedAny);
    }

    [Fact]
    public async Task ProcessAsync_DuplicateIdempotencyKey_SkipsSecondMessage()
    {
        var store = new FakeOutboxStore();
        var dlq = new DeadLetterInMemoryStore();
        var dispatcher = new FakeDispatcher();
        var options = new OutboxOptions(MaxRetries: 3);
        var service = new OutboxRelayService(store, dispatcher, dlq, options, NullLogger<OutboxRelayService>.Instance);

        var msg1 = new OutboxMessage(Guid.NewGuid(), "Test", "{\"v\":1}", DateTimeOffset.UtcNow, IdempotencyKey: "key-1");
        var msg2 = new OutboxMessage(Guid.NewGuid(), "Test", "{\"v\":2}", DateTimeOffset.UtcNow.AddSeconds(1), IdempotencyKey: "key-1");
        store.Messages.Add(msg1);
        store.Messages.Add(msg2);

        await service.ProcessAsync(CancellationToken.None);

        Assert.Single(dispatcher.Dispatched);
        Assert.Equal(msg1.Id, dispatcher.Dispatched[0].Id);
    }

    [Fact]
    public async Task ProcessAsync_PriorityOrdering_HigherPriorityFirst()
    {
        var store = new FakeOutboxStore();
        var dlq = new DeadLetterInMemoryStore();
        var dispatcher = new FakeDispatcher();
        var options = new OutboxOptions(MaxRetries: 3);
        var service = new OutboxRelayService(store, dispatcher, dlq, options, NullLogger<OutboxRelayService>.Instance);

        var low = new OutboxMessage(Guid.NewGuid(), "Low", "{}", DateTimeOffset.UtcNow, Priority: MessagePriority.Low);
        var critical = new OutboxMessage(Guid.NewGuid(), "Critical", "{}", DateTimeOffset.UtcNow, Priority: MessagePriority.Critical);
        var normal = new OutboxMessage(Guid.NewGuid(), "Normal", "{}", DateTimeOffset.UtcNow, Priority: MessagePriority.Normal);
        store.Messages.Add(low);
        store.Messages.Add(critical);
        store.Messages.Add(normal);

        await service.ProcessAsync(CancellationToken.None);

        Assert.Equal(3, dispatcher.Dispatched.Count);
        Assert.Equal(critical.Id, dispatcher.Dispatched[0].Id);
        Assert.Equal(normal.Id, dispatcher.Dispatched[1].Id);
        Assert.Equal(low.Id, dispatcher.Dispatched[2].Id);
    }

    [Fact]
    public async Task ProcessAsync_DiagnosticEvents_RaisedOnDispatch()
    {
        var store = new FakeOutboxStore();
        var dlq = new DeadLetterInMemoryStore();
        var dispatcher = new FakeDispatcher();
        var options = new OutboxOptions(MaxRetries: 3);
        var service = new OutboxRelayService(store, dispatcher, dlq, options, NullLogger<OutboxRelayService>.Instance);

        var dispatched = new List<OutboxMessage>();
        OutboxDiagnostics.MessageDispatched += m => dispatched.Add(m);

        var message = new OutboxMessage(Guid.NewGuid(), "Test", "{}", DateTimeOffset.UtcNow);
        store.Messages.Add(message);

        await service.ProcessAsync(CancellationToken.None);

        Assert.Single(dispatched);
        Assert.Equal(message.Id, dispatched[0].Id);
    }

    [Fact]
    public async Task ProcessAsync_FailedDispatch_RaisesFailedEvent()
    {
        var store = new FakeOutboxStore();
        var dlq = new DeadLetterInMemoryStore();
        var dispatcher = new FakeDispatcher { ShouldFail = true };
        var options = new OutboxOptions(MaxRetries: 3);
        var service = new OutboxRelayService(store, dispatcher, dlq, options, NullLogger<OutboxRelayService>.Instance);

        var failed = new List<OutboxMessage>();
        OutboxDiagnostics.MessageFailed += m => failed.Add(m);

        var message = new OutboxMessage(Guid.NewGuid(), "Test", "{}", DateTimeOffset.UtcNow);
        store.Messages.Add(message);

        await service.ProcessAsync(CancellationToken.None);

        Assert.Single(failed);
        Assert.Equal(message.Id, failed[0].Id);
    }

    private sealed class FakeOutboxStore : IOutboxStore
    {
        public List<OutboxMessage> Messages { get; } = new();
        private readonly HashSet<Guid> _processed = new();

        public Task SaveAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<OutboxMessage> pending = Messages
                .Where(m => !_processed.Contains(m.Id))
                .Take(batchSize)
                .ToList()
                .AsReadOnly();
            return Task.FromResult(pending);
        }

        public Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _processed.Add(id);
            return Task.CompletedTask;
        }

        public Task MarkFailedAsync(Guid id, string error, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDispatcher : IOutboxDispatcher
    {
        public List<OutboxMessage> Dispatched { get; } = new();
        public bool DispatchedAny => Dispatched.Count > 0;
        public bool ShouldFail { get; set; }

        public Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            if (ShouldFail)
            {
                throw new InvalidOperationException("Dispatch failed");
            }

            Dispatched.Add(message);
            return Task.CompletedTask;
        }
    }
}
