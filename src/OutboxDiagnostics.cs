namespace Philiprehberger.Outbox;

/// <summary>
/// Provides diagnostic events for observing the outbox message lifecycle.
/// Subscribe to these events for logging, metrics, or alerting.
/// </summary>
public static class OutboxDiagnostics
{
    /// <summary>
    /// Raised when a message is enqueued in the outbox store.
    /// </summary>
    public static event Action<OutboxMessage>? MessageEnqueued;

    /// <summary>
    /// Raised when a message is successfully dispatched.
    /// </summary>
    public static event Action<OutboxMessage>? MessageDispatched;

    /// <summary>
    /// Raised when a message dispatch fails.
    /// </summary>
    public static event Action<OutboxMessage>? MessageFailed;

    /// <summary>
    /// Raised when a message is moved to the dead letter queue.
    /// </summary>
    public static event Action<OutboxMessage>? MessageDeadLettered;

    /// <summary>
    /// Invokes the <see cref="MessageEnqueued"/> event.
    /// </summary>
    /// <param name="message">The enqueued message.</param>
    internal static void OnMessageEnqueued(OutboxMessage message) => MessageEnqueued?.Invoke(message);

    /// <summary>
    /// Invokes the <see cref="MessageDispatched"/> event.
    /// </summary>
    /// <param name="message">The dispatched message.</param>
    internal static void OnMessageDispatched(OutboxMessage message) => MessageDispatched?.Invoke(message);

    /// <summary>
    /// Invokes the <see cref="MessageFailed"/> event.
    /// </summary>
    /// <param name="message">The failed message.</param>
    internal static void OnMessageFailed(OutboxMessage message) => MessageFailed?.Invoke(message);

    /// <summary>
    /// Invokes the <see cref="MessageDeadLettered"/> event.
    /// </summary>
    /// <param name="message">The dead-lettered message.</param>
    internal static void OnMessageDeadLettered(OutboxMessage message) => MessageDeadLettered?.Invoke(message);

    /// <summary>
    /// Removes all event subscribers. Useful for test cleanup.
    /// </summary>
    internal static void Reset()
    {
        MessageEnqueued = null;
        MessageDispatched = null;
        MessageFailed = null;
        MessageDeadLettered = null;
    }
}
