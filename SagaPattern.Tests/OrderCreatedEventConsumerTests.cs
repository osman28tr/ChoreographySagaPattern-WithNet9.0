using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Abstract;
using Shared.Events;
using Shared.Messages;
using Stock.API.Consumers;
using Stock.API.Models;
using StockModel = Stock.API.Models.Stock;

namespace SagaPattern.Tests;

/// <summary>
/// OrderCreatedEventConsumer'ın entegrasyon testleri.
/// MassTransit InMemory test harness kullanılarak mesajlaşma davranışı doğrulanır.
/// </summary>
public class OrderCreatedEventConsumerTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;

    public async Task InitializeAsync()
    {
        var dbName = Guid.NewGuid().ToString();
        _provider = new ServiceCollection()
            .AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase(dbName))
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<OrderCreatedEventConsumer>();
            })
            .BuildServiceProvider(true);

        // Seed stock data
        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Stocks.AddRange(
            new StockModel { ProductId = 1, Count = 100 },
            new StockModel { ProductId = 2, Count = 50 }
        );
        await db.SaveChangesAsync();

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task Consume_WhenStockIsSufficient_PublishesStockReservedEvent()
    {
        var correlationId = Guid.NewGuid();
        var orderItems = new List<OrderItemMessage>
        {
            new() { ProductId = 1, Count = 5 }
        };

        await _harness.Bus.Publish<IOrderCreatedEvent>(new OrderCreatedEvent(correlationId)
        {
            OrderItems = orderItems
        });

        Assert.True(await _harness.Consumed.Any<IOrderCreatedEvent>());
        Assert.True(await _harness.Published.Any<StockReservedEvent>());
        Assert.False(await _harness.Published.Any<StockNotReservedEvent>());
    }

    [Fact]
    public async Task Consume_WhenStockIsInsufficient_PublishesStockNotReservedEvent()
    {
        var correlationId = Guid.NewGuid();
        var orderItems = new List<OrderItemMessage>
        {
            new() { ProductId = 1, Count = 200 } // Mevcut stok: 100, talep: 200
        };

        await _harness.Bus.Publish<IOrderCreatedEvent>(new OrderCreatedEvent(correlationId)
        {
            OrderItems = orderItems
        });

        Assert.True(await _harness.Consumed.Any<IOrderCreatedEvent>());
        Assert.True(await _harness.Published.Any<StockNotReservedEvent>());
        Assert.False(await _harness.Published.Any<StockReservedEvent>());
    }

    [Fact]
    public async Task Consume_WhenStockIsSufficient_DecreasesStockCount()
    {
        var correlationId = Guid.NewGuid();
        var orderItems = new List<OrderItemMessage>
        {
            new() { ProductId = 2, Count = 10 }
        };

        await _harness.Bus.Publish<IOrderCreatedEvent>(new OrderCreatedEvent(correlationId)
        {
            OrderItems = orderItems
        });

        Assert.True(await _harness.Consumed.Any<IOrderCreatedEvent>());

        using var scope = _provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stock = await db.Stocks.FirstOrDefaultAsync(s => s.ProductId == 2);
        Assert.NotNull(stock);
        Assert.Equal(40, stock.Count); // 50 - 10 = 40
    }

    [Fact]
    public async Task Consume_WhenOneProductHasInsufficientStock_PublishesStockNotReservedEvent()
    {
        var correlationId = Guid.NewGuid();
        var orderItems = new List<OrderItemMessage>
        {
            new() { ProductId = 1, Count = 5  }, // Yeterli stok (100 > 5)
            new() { ProductId = 2, Count = 999 } // Yetersiz stok (50 < 999)
        };

        await _harness.Bus.Publish<IOrderCreatedEvent>(new OrderCreatedEvent(correlationId)
        {
            OrderItems = orderItems
        });

        Assert.True(await _harness.Consumed.Any<IOrderCreatedEvent>());
        Assert.True(await _harness.Published.Any<StockNotReservedEvent>());
        Assert.False(await _harness.Published.Any<StockReservedEvent>());
    }
}
