namespace Philiprehberger.Outbox;

/// <summary>
/// Represents a message stored in the transactional outbox.
/// </summary>
/// <param name="Id">Unique identifier for the message.</param>
/// <param name="Type">The message type discriminator (e.g., fully-qualified event name).</param>
/// <param name="Payload">The serialized message payload (JSON).</param>
/// <param name="CreatedAt">Timestamp when the message was created.</param>
/// <param name="ProcessedAt">Timestamp when the message was successfully dispatched, or <c>null</c> if pending.</param>
/// <param name="Error">The error message from the last failed dispatch attempt, or <c>null</c> if no error.</param>
/// <param name="RetryCount">The number of dispatch attempts that have been made.</param>
/// <param name="IdempotencyKey">An optional key for deduplication. Messages with the same key are dispatched only once.</param>
/// <param name="Priority">The dispatch priority level. Higher-priority messages are processed first.</param>
public record OutboxMessage(
    Guid Id,
    string Type,
    string Payload,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt = null,
    string? Error = null,
    int RetryCount = 0,
    string? IdempotencyKey = null,
    MessagePriority Priority = MessagePriority.Normal);
