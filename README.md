# Philiprehberger.Outbox

[![CI](https://github.com/philiprehberger/dotnet-outbox/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-outbox/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.Outbox.svg)](https://www.nuget.org/packages/Philiprehberger.Outbox)
[![GitHub release](https://img.shields.io/github/v/release/philiprehberger/dotnet-outbox)](https://github.com/philiprehberger/dotnet-outbox/releases)
[![Last updated](https://img.shields.io/github/last-commit/philiprehberger/dotnet-outbox)](https://github.com/philiprehberger/dotnet-outbox/commits/main)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-outbox)](LICENSE)
[![Bug Reports](https://img.shields.io/github/issues/philiprehberger/dotnet-outbox/bug)](https://github.com/philiprehberger/dotnet-outbox/issues?q=is%3Aissue+is%3Aopen+label%3Abug)
[![Feature Requests](https://img.shields.io/github/issues/philiprehberger/dotnet-outbox/enhancement)](https://github.com/philiprehberger/dotnet-outbox/issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement)
[![Sponsor](https://img.shields.io/badge/sponsor-GitHub%20Sponsors-ec6cb9)](https://github.com/sponsors/philiprehberger)

Transactional outbox pattern implementation for reliable event/message publishing.

## Installation

```bash
dotnet add package Philiprehberger.Outbox
```

## Usage

```csharp
using Philiprehberger.Outbox;

builder.Services.AddOutbox(options =>
{
    options = options with
    {
        PollingInterval = TimeSpan.FromSeconds(10),
        BatchSize = 50,
        MaxRetries = 5
    };
});

builder.Services.AddSingleton<IOutboxStore, MyEfCoreOutboxStore>();
builder.Services.AddSingleton<IOutboxDispatcher, MyRabbitMqDispatcher>();
```

### Saving Messages

```csharp
using Philiprehberger.Outbox;

var message = new OutboxMessage(
    Id: Guid.NewGuid(),
    Type: "OrderPlaced",
    Payload: JsonSerializer.Serialize(order),
    CreatedAt: DateTimeOffset.UtcNow);

await outboxStore.SaveAsync(message);
```

### Message Priority

```csharp
using Philiprehberger.Outbox;

var urgent = new OutboxMessage(
    Id: Guid.NewGuid(),
    Type: "PaymentFailed",
    Payload: JsonSerializer.Serialize(payment),
    CreatedAt: DateTimeOffset.UtcNow,
    Priority: MessagePriority.Critical);

await outboxStore.SaveAsync(urgent);
```

### Deduplication with Idempotency Keys

```csharp
using Philiprehberger.Outbox;

var message = new OutboxMessage(
    Id: Guid.NewGuid(),
    Type: "OrderPlaced",
    Payload: JsonSerializer.Serialize(order),
    CreatedAt: DateTimeOffset.UtcNow,
    IdempotencyKey: $"order-placed-{order.Id}");

await outboxStore.SaveAsync(message);
```

### Dead Letter Queue

```csharp
using Philiprehberger.Outbox;

// Default in-memory DLQ is registered automatically.
// Replace with your own implementation for production:
builder.Services.AddSingleton<IDeadLetterStore, MyDatabaseDeadLetterStore>();

// Inspect dead-lettered messages:
var dlq = serviceProvider.GetRequiredService<IDeadLetterStore>();
var failed = await dlq.GetAllAsync();
```

### Diagnostic Events

```csharp
using Philiprehberger.Outbox;

OutboxDiagnostics.MessageDispatched += msg =>
    Console.WriteLine($"Dispatched: {msg.Id} ({msg.Type})");

OutboxDiagnostics.MessageDeadLettered += msg =>
    Console.WriteLine($"Dead-lettered: {msg.Id} after {msg.RetryCount} retries");
```

## API

### `OutboxMessage`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Id` | `Guid` | | Unique message identifier |
| `Type` | `string` | | Message type discriminator |
| `Payload` | `string` | | Serialized message payload (JSON) |
| `CreatedAt` | `DateTimeOffset` | | When the message was created |
| `ProcessedAt` | `DateTimeOffset?` | `null` | When the message was dispatched |
| `Error` | `string?` | `null` | Last failure error message |
| `RetryCount` | `int` | `0` | Number of dispatch attempts |
| `IdempotencyKey` | `string?` | `null` | Deduplication key (duplicates are skipped) |
| `Priority` | `MessagePriority` | `Normal` | Dispatch priority level |

### `MessagePriority`

| Value | Description |
|-------|-------------|
| `Low` | Dispatched after all higher-priority messages |
| `Normal` | Default priority |
| `High` | Dispatched before Normal and Low |
| `Critical` | Dispatched before all other messages |

### `IOutboxStore`

| Method | Description |
|--------|-------------|
| `SaveAsync(message, ct)` | Persist a new outbox message |
| `GetPendingAsync(batchSize, ct)` | Retrieve unprocessed messages |
| `MarkProcessedAsync(id, ct)` | Mark a message as dispatched |
| `MarkFailedAsync(id, error, ct)` | Mark a message as failed |

### `IOutboxDispatcher`

| Method | Description |
|--------|-------------|
| `DispatchAsync(message, ct)` | Dispatch a message to its destination |

### `IDeadLetterStore`

| Method | Description |
|--------|-------------|
| `AddAsync(message, ct)` | Add a failed message to the dead letter queue |
| `GetAllAsync(ct)` | Retrieve all dead-lettered messages |

### `OutboxOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PollingInterval` | `TimeSpan?` | 5 seconds | How often the relay polls for pending messages |
| `BatchSize` | `int` | `100` | Max messages per polling cycle |
| `MaxRetries` | `int` | `3` | Max dispatch attempts before dead-lettering |

### `OutboxDiagnostics`

| Event | Description |
|-------|-------------|
| `MessageEnqueued` | Raised when a message is saved to the outbox |
| `MessageDispatched` | Raised when a message is successfully dispatched |
| `MessageFailed` | Raised when a dispatch attempt fails |
| `MessageDeadLettered` | Raised when a message is moved to the dead letter queue |

### `OutboxServiceCollectionExtensions`

| Method | Description |
|--------|-------------|
| `AddOutbox(configure?)` | Register the outbox relay service, options, and default DLQ |

## Development

```bash
dotnet build src/Philiprehberger.Outbox.csproj --configuration Release
```

## Support

If you find this package useful, consider giving it a star on GitHub — it helps motivate continued maintenance and development.

[![LinkedIn](https://img.shields.io/badge/Philip%20Rehberger-LinkedIn-0A66C2?logo=linkedin)](https://www.linkedin.com/in/philiprehberger)
[![More packages](https://img.shields.io/badge/more-open%20source%20packages-blue)](https://philiprehberger.com/open-source-packages)

## License

[MIT](LICENSE)
