using Xunit;
using Philiprehberger.Outbox;

namespace Philiprehberger.Outbox.Tests;

public class OutboxMessageTests
{
    [Fact]
    public void Constructor_WithRequiredParameters_SetsProperties()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var message = new OutboxMessage(id, "OrderCreated", "{\"orderId\":1}", createdAt);

        Assert.Equal(id, message.Id);
        Assert.Equal("OrderCreated", message.Type);
        Assert.Equal("{\"orderId\":1}", message.Payload);
        Assert.Equal(createdAt, message.CreatedAt);
    }

    [Fact]
    public void Constructor_WithDefaults_HasNullProcessedAtAndError()
    {
        var message = new OutboxMessage(Guid.NewGuid(), "Test", "{}", DateTimeOffset.UtcNow);

        Assert.Null(message.ProcessedAt);
        Assert.Null(message.Error);
        Assert.Equal(0, message.RetryCount);
        Assert.Null(message.IdempotencyKey);
        Assert.Equal(MessagePriority.Normal, message.Priority);
    }

    [Fact]
    public void Constructor_WithOptionalParameters_SetsAllProperties()
    {
        var processedAt = DateTimeOffset.UtcNow;

        var message = new OutboxMessage(
            Guid.NewGuid(),
            "Test",
            "{}",
            DateTimeOffset.UtcNow,
            ProcessedAt: processedAt,
            Error: "some error",
            RetryCount: 3,
            IdempotencyKey: "dedup-123",
            Priority: MessagePriority.High);

        Assert.Equal(processedAt, message.ProcessedAt);
        Assert.Equal("some error", message.Error);
        Assert.Equal(3, message.RetryCount);
        Assert.Equal("dedup-123", message.IdempotencyKey);
        Assert.Equal(MessagePriority.High, message.Priority);
    }

    [Fact]
    public void Equality_TwoMessagesWithSameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var a = new OutboxMessage(id, "Test", "{}", createdAt);
        var b = new OutboxMessage(id, "Test", "{}", createdAt);

        Assert.Equal(a, b);
    }
}
