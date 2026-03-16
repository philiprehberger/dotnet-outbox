namespace Philiprehberger.Outbox;

/// <summary>
/// Dispatches outbox messages to their destination (e.g., message broker, HTTP endpoint).
/// </summary>
public interface IOutboxDispatcher
{
    /// <summary>
    /// Dispatches a single outbox message.
    /// </summary>
    /// <param name="message">The outbox message to dispatch.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that completes when the message has been dispatched.</returns>
    Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
