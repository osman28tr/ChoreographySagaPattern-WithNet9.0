using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using SagaStateMachineWorkerService.Models;
using Shared.Abstract;
using Shared.Events;
using Shared.Messages;

namespace SagaPattern.Tests;

/// <summary>
/// OrderStateMachine'in durum geçişlerini doğrulayan entegrasyon testleri.
/// MassTransit InMemory test harness ve InMemorySagaRepository kullanılır.
/// </summary>
public class OrderStateMachineTests : IAsyncLifetime
{
    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;
    private ISagaStateMachineTestHarness<OrderStateMachine, OrderStateInstance> _sagaHarness = null!;

    public async Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
                   .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();

        _sagaHarness = _harness.GetSagaStateMachineHarness<OrderStateMachine, OrderStateInstance>();
    }

    public async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task OrderCreatedRequestEvent_TransitionsStateTo_OrderCreated()
    {
        var requestEvent = BuildOrderCreatedRequestEvent(orderId: 1, buyerId: "buyer-1");

        await _harness.Bus.Publish<IOrderCreatedRequestEvent>(requestEvent);

        var sagaIds = await _sagaHarness.Exists(x => x.OrderId == 1, x => x.OrderCreated);
        Assert.NotEmpty(sagaIds);
    }

    [Fact]
    public async Task OrderCreatedRequestEvent_PublishesOrderCreatedEvent()
    {
        await _harness.Bus.Publish<IOrderCreatedRequestEvent>(
            BuildOrderCreatedRequestEvent(orderId: 2, buyerId: "buyer-2"));

        Assert.True(await _harness.Published.Any<OrderCreatedEvent>());
    }

    [Fact]
    public async Task OrderCreatedRequestEvent_PersistsBuyerAndOrderIdInSagaInstance()
    {
        var requestEvent = BuildOrderCreatedRequestEvent(orderId: 3, buyerId: "buyer-3");

        await _harness.Bus.Publish<IOrderCreatedRequestEvent>(requestEvent);

        var sagaIds = await _sagaHarness.Exists(x => x.OrderId == 3, x => x.OrderCreated);
        Assert.NotEmpty(sagaIds);

        var instance = await _sagaHarness.Match(x => x.OrderId == 3 && x.BuyerId == "buyer-3");
        Assert.NotNull(instance);
    }

    [Fact]
    public async Task StockReservedEvent_WhenInOrderCreatedState_TransitionsTo_StockReserved()
    {
        // 1. Siparişi oluştur → OrderCreated durumuna geç
        var requestEvent = BuildOrderCreatedRequestEvent(orderId: 4, buyerId: "buyer-4");
        await _harness.Bus.Publish<IOrderCreatedRequestEvent>(requestEvent);

        var sagaIds = await _sagaHarness.Exists(x => x.OrderId == 4, x => x.OrderCreated);
        Assert.NotEmpty(sagaIds);
        var sagaId = sagaIds.First();

        // 2. Stok rezerve edildi olayını yayınla → StockReserved durumuna geç
        await _harness.Bus.Publish<IStockReservedEvent>(new StockReservedEvent(sagaId)
        {
            OrderItems = new List<OrderItemMessage> { new() { ProductId = 1, Count = 2 } }
        });

        var stockReservedId = await _sagaHarness.Exists(sagaId, x => x.StockReserved);
        Assert.NotNull(stockReservedId);
    }

    [Fact]
    public async Task StockReservedEvent_WhenInOrderCreatedState_SendsPaymentRequest()
    {
        var requestEvent = BuildOrderCreatedRequestEvent(orderId: 5, buyerId: "buyer-5");
        await _harness.Bus.Publish<IOrderCreatedRequestEvent>(requestEvent);

        var sagaIds = await _sagaHarness.Exists(x => x.OrderId == 5, x => x.OrderCreated);
        Assert.NotEmpty(sagaIds);
        var sagaId = sagaIds.First();

        await _harness.Bus.Publish<IStockReservedEvent>(new StockReservedEvent(sagaId)
        {
            OrderItems = new List<OrderItemMessage> { new() { ProductId = 1, Count = 3 } }
        });

        Assert.True(await _harness.Sent.Any<StockReservedRequestPaymentEvent>());
    }

    private static OrderCreatedRequestEvent BuildOrderCreatedRequestEvent(int orderId, string buyerId) =>
        new()
        {
            OrderId = orderId,
            BuyerId = buyerId,
            Payment = new PaymentMessage
            {
                CardName = "Test User",
                CardNumber = "4111111111111111",
                Expiration = "12/27",
                CVV = "123",
                TotalPrice = 500m
            },
            OrderItems = new List<OrderItemMessage>
            {
                new() { ProductId = 1, Count = 3 }
            }
        };
}
