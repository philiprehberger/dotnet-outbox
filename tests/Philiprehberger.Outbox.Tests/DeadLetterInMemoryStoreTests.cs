using Xunit;
using Philiprehberger.Outbox;

namespace Philiprehberger.Outbox.Tests;

public class DeadLetterInMemoryStoreTests
{
    [Fact]
    public async Task AddAsync_StoresMessage()
    {
        var store = new DeadLetterInMemoryStore();
        var message = new OutboxMessage(Guid.NewGuid(), "Test", "{}", DateTimeOffset.UtcNow);

        await store.AddAsync(message);

        var all = await store.GetAllAsync();
        Assert.Single(all);
        Assert.Equal(message.Id, all[0].Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
    {
        var store = new DeadLetterInMemoryStore();

        var all = await store.GetAllAsync();

        Assert.Empty(all);
    }

    [Fact]
    public async Task AddAsync_MultipleMessages_ReturnsAll()
    {
        var store = new DeadLetterInMemoryStore();
        var msg1 = new OutboxMessage(Guid.NewGuid(), "A", "{}", DateTimeOffset.UtcNow);
        var msg2 = new OutboxMessage(Guid.NewGuid(), "B", "{}", DateTimeOffset.UtcNow);

        await store.AddAsync(msg1);
        await store.AddAsync(msg2);

        var all = await store.GetAllAsync();
        Assert.Equal(2, all.Count);
    }
}
