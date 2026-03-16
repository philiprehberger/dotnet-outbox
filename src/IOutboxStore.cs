namespace Philiprehberger.Outbox;

/// <summary>
/// Persistence abstraction for outbox messages.
/// Implement this interface to back the outbox with any storage mechanism
/// (e.g., EF Core, Dapper, in-memory).
/// </summary>
public interface IOutboxStore
{
    /// <summary>
    /// Persists a new outbox message.
    /// </summary>
    /// <param name="message">The message to save.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SaveAsync(OutboxMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a batch of unprocessed messages ordered by creation time.
    /// </summary>
    /// <param name="batchSize">Maximum number of messages to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of pending outbox messages.</returns>
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as successfully processed.
    /// </summary>
    /// <param name="id">The identifier of the message to mark.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as failed with an error description.
    /// </summary>
    /// <param name="id">The identifier of the message to mark.</param>
    /// <param name="error">A description of the failure.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task MarkFailedAsync(Guid id, string error, CancellationToken cancellationToken = default);
}
