namespace Philiprehberger.Outbox;

/// <summary>
/// Persistence abstraction for dead-lettered outbox messages.
/// Messages that exceed the maximum retry count are moved here
/// instead of being silently dropped.
/// </summary>
public interface IDeadLetterStore
{
    /// <summary>
    /// Adds a failed message to the dead letter queue.
    /// </summary>
    /// <param name="message">The message that exceeded max retries.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all messages in the dead letter queue.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of dead-lettered messages.</returns>
    Task<IReadOnlyList<OutboxMessage>> GetAllAsync(CancellationToken cancellationToken = default);
}
