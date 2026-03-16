# Philiprehberger.Outbox

[![CI](https://github.com/philiprehberger/dotnet-outbox/actions/workflows/ci.yml/badge.svg)](https://github.com/philiprehberger/dotnet-outbox/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Philiprehberger.Outbox.svg)](https://www.nuget.org/packages/Philiprehberger.Outbox)
[![License](https://img.shields.io/github/license/philiprehberger/dotnet-outbox)](LICENSE)

Transactional outbox pattern implementation for reliable event/message publishing.

## Install

```bash
dotnet add package Philiprehberger.Outbox
```

## Usage

### 1. Register the outbox

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

// Register your own implementations
builder.Services.AddSingleton<IOutboxStore, MyEfCoreOutboxStore>();
builder.Services.AddSingleton<IOutboxDispatcher, MyRabbitMqDispatcher>();
```

### 2. Save messages to the outbox

```csharp
using Philiprehberger.Outbox;

public class OrderService
{
    private readonly IOutboxStore _outbox;

    public OrderService(IOutboxStore outbox) => _outbox = outbox;

    public async Task PlaceOrderAsync(Order order, CancellationToken ct)
    {
        // Save order to your database...

        // Then save the event to the outbox (in the same transaction)
        var message = new OutboxMessage(
            Id: Guid.NewGuid(),
            Type: "OrderPlaced",
            Payload: JsonSerializer.Serialize(order),
            CreatedAt: DateTimeOffset.UtcNow);

        await _outbox.SaveAsync(message, ct);
    }
}
```

### 3. Implement the store and dispatcher

Implement `IOutboxStore` to persist messages in your database and `IOutboxDispatcher` to publish them to your message broker. The `OutboxRelayService` runs in the background, polling for pending messages and dispatching them automatically.

## API

### `OutboxMessage`

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Unique message identifier |
| `Type` | `string` | Message type discriminator |
| `Payload` | `string` | Serialized message payload (JSON) |
| `CreatedAt` | `DateTimeOffset` | When the message was created |
| `ProcessedAt` | `DateTimeOffset?` | When the message was dispatched |
| `Error` | `string?` | Last failure error message |
| `RetryCount` | `int` | Number of dispatch attempts |

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

### `OutboxOptions`

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PollingInterval` | `TimeSpan?` | 5 seconds | How often the relay polls for pending messages |
| `BatchSize` | `int` | 100 | Max messages per polling cycle |
| `MaxRetries` | `int` | 3 | Max dispatch attempts before abandoning |

### `OutboxServiceCollectionExtensions`

| Method | Description |
|--------|-------------|
| `AddOutbox(configure?)` | Register the outbox relay service and options |

## Development

```bash
dotnet build src/Philiprehberger.Outbox.csproj --configuration Release
```

## License

MIT
