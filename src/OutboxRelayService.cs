using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Philiprehberger.Outbox;

/// <summary>
/// Background hosted service that polls the outbox store for pending messages
/// and dispatches them via the configured <see cref="IOutboxDispatcher"/>.
/// </summary>
public sealed class OutboxRelayService : IHostedService, IDisposable
{
    private readonly IOutboxStore _store;
    private readonly IOutboxDispatcher _dispatcher;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxRelayService> _logger;
    private Timer? _timer;

    /// <summary>
    /// Initializes a new instance of <see cref="OutboxRelayService"/>.
    /// </summary>
    /// <param name="store">The outbox message store.</param>
    /// <param name="dispatcher">The message dispatcher.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Logger instance.</param>
    public OutboxRelayService(
        IOutboxStore store,
        IOutboxDispatcher dispatcher,
        OutboxOptions options,
        ILogger<OutboxRelayService> logger)
    {
        _store = store;
        _dispatcher = dispatcher;
        _options = options;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Outbox relay started. Polling every {Interval}s, batch size {BatchSize}, max retries {MaxRetries}",
            _options.EffectivePollingInterval.TotalSeconds,
            _options.BatchSize,
            _options.MaxRetries);

        _timer = new Timer(
            _ => _ = ProcessAsync(CancellationToken.None),
            null,
            TimeSpan.Zero,
            _options.EffectivePollingInterval);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Outbox relay stopping");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _timer?.Dispose();
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        try
        {
            var messages = await _store.GetPendingAsync(_options.BatchSize, cancellationToken);

            if (messages.Count == 0)
            {
                return;
            }

            _logger.LogDebug("Processing {Count} pending outbox messages", messages.Count);

            foreach (var message in messages)
            {
                if (message.RetryCount >= _options.MaxRetries)
                {
                    _logger.LogWarning(
                        "Message {Id} exceeded max retries ({MaxRetries}), skipping",
                        message.Id,
                        _options.MaxRetries);
                    continue;
                }

                try
                {
                    await _dispatcher.DispatchAsync(message, cancellationToken);
                    await _store.MarkProcessedAsync(message.Id, cancellationToken);

                    _logger.LogDebug("Message {Id} dispatched successfully", message.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to dispatch message {Id} (attempt {Attempt})", message.Id, message.RetryCount + 1);
                    await _store.MarkFailedAsync(message.Id, ex.Message, cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during outbox relay processing");
        }
    }
}
