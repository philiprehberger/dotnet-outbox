using System.Collections.Concurrent;

namespace Philiprehberger.Outbox;

/// <summary>
/// In-memory implementation of <see cref="IDeadLetterStore"/>.
/// Useful for testing and development. Not suitable for production use
/// because messages are lost when the process exits.
/// </summary>
public sealed class DeadLetterInMemoryStore : IDeadLetterStore
{
    private readonly ConcurrentBag<OutboxMessage> _messages = new();

    /// <inheritdoc />
    public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _messages.Add(message);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<OutboxMessage>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OutboxMessage> result = _messages.ToList().AsReadOnly();
        return Task.FromResult(result);
    }
}
