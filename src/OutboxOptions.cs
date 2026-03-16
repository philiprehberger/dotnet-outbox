namespace Philiprehberger.Outbox;

/// <summary>
/// Configuration options for the outbox relay service.
/// </summary>
/// <param name="PollingInterval">How often the relay polls for pending messages. Defaults to 5 seconds.</param>
/// <param name="BatchSize">Maximum number of messages to process per polling cycle. Defaults to 100.</param>
/// <param name="MaxRetries">Maximum number of dispatch attempts before a message is abandoned. Defaults to 3.</param>
public record OutboxOptions(
    TimeSpan? PollingInterval = null,
    int BatchSize = 100,
    int MaxRetries = 3)
{
    /// <summary>
    /// The effective polling interval, defaulting to 5 seconds when not specified.
    /// </summary>
    public TimeSpan EffectivePollingInterval => PollingInterval ?? TimeSpan.FromSeconds(5);
}
