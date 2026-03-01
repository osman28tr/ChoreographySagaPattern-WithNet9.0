using Shared;
using Shared.Events;
using Shared.Messages;

namespace SagaPattern.Tests;

/// <summary>
/// Shared katmanındaki event ve mesaj sınıflarının birim testleri.
/// </summary>
public class SharedEventsTests
{
    [Fact]
    public void OrderCreatedEvent_CorrelationId_ShouldMatch()
    {
        var correlationId = Guid.NewGuid();
        var orderEvent = new OrderCreatedEvent(correlationId)
        {
            OrderItems = new List<OrderItemMessage>()
        };

        Assert.Equal(correlationId, orderEvent.CorrelationId);
    }

    [Fact]
    public void OrderCreatedEvent_OrderItems_ShouldBeSet()
    {
        var correlationId = Guid.NewGuid();
        var items = new List<OrderItemMessage>
        {
            new() { ProductId = 1, Count = 3 },
            new() { ProductId = 2, Count = 1 }
        };

        var orderEvent = new OrderCreatedEvent(correlationId) { OrderItems = items };

        Assert.Equal(2, orderEvent.OrderItems.Count);
        Assert.Contains(orderEvent.OrderItems, i => i.ProductId == 1 && i.Count == 3);
        Assert.Contains(orderEvent.OrderItems, i => i.ProductId == 2 && i.Count == 1);
    }

    [Fact]
    public void StockReservedEvent_CorrelationId_ShouldMatch()
    {
        var correlationId = Guid.NewGuid();
        var stockEvent = new StockReservedEvent(correlationId)
        {
            OrderItems = new List<OrderItemMessage>()
        };

        Assert.Equal(correlationId, stockEvent.CorrelationId);
    }

    [Fact]
    public void StockNotReservedEvent_Reason_ShouldBeSet()
    {
        var correlationId = Guid.NewGuid();
        var stockNotReservedEvent = new StockNotReservedEvent(correlationId)
        {
            Reason = "Not enough stock"
        };

        Assert.Equal(correlationId, stockNotReservedEvent.CorrelationId);
        Assert.Equal("Not enough stock", stockNotReservedEvent.Reason);
    }

    [Fact]
    public void PaymentCompletedEvent_Properties_ShouldBeSet()
    {
        var paymentEvent = new PaymentCompletedEvent
        {
            OrderId = 42,
            BuyerId = "buyer-001"
        };

        Assert.Equal(42, paymentEvent.OrderId);
        Assert.Equal("buyer-001", paymentEvent.BuyerId);
    }

    [Fact]
    public void PaymentFailedEvent_Properties_ShouldBeSet()
    {
        var items = new List<OrderItemMessage> { new() { ProductId = 3, Count = 2 } };
        var paymentFailedEvent = new PaymentFailedEvent
        {
            OrderId = 10,
            BuyerId = "buyer-002",
            Message = "Insufficient funds",
            OrderItems = items
        };

        Assert.Equal(10, paymentFailedEvent.OrderId);
        Assert.Equal("buyer-002", paymentFailedEvent.BuyerId);
        Assert.Equal("Insufficient funds", paymentFailedEvent.Message);
        Assert.Single(paymentFailedEvent.OrderItems);
    }

    [Fact]
    public void OrderCreatedRequestEvent_Properties_ShouldBeSet()
    {
        var payment = new PaymentMessage
        {
            CardName = "Test User",
            CardNumber = "1234567890123456",
            Expiration = "12/26",
            CVV = "123",
            TotalPrice = 250.00m
        };
        var items = new List<OrderItemMessage> { new() { ProductId = 1, Count = 5 } };

        var requestEvent = new OrderCreatedRequestEvent
        {
            OrderId = 7,
            BuyerId = "buyer-003",
            Payment = payment,
            OrderItems = items
        };

        Assert.Equal(7, requestEvent.OrderId);
        Assert.Equal("buyer-003", requestEvent.BuyerId);
        Assert.Equal(250.00m, requestEvent.Payment.TotalPrice);
        Assert.Single(requestEvent.OrderItems);
    }

    [Fact]
    public void StockReservedRequestPaymentEvent_Properties_ShouldBeSet()
    {
        var correlationId = Guid.NewGuid();
        var payment = new PaymentMessage { TotalPrice = 100m, CardName = "John Doe" };
        var items = new List<OrderItemMessage> { new() { ProductId = 2, Count = 4 } };

        var paymentRequestEvent = new StockReservedRequestPaymentEvent(correlationId)
        {
            PaymentMessage = payment,
            OrderItems = items
        };

        Assert.Equal(correlationId, paymentRequestEvent.CorrelationId);
        Assert.Equal(100m, paymentRequestEvent.PaymentMessage.TotalPrice);
        Assert.Single(paymentRequestEvent.OrderItems);
    }

    [Fact]
    public void RabbitMQSettingsConst_QueueNames_ShouldBeCorrect()
    {
        Assert.Equal("stock-reserved-event-queue", RabbitMQSettingsConst.StockReservedEventQueue);
        Assert.Equal("stock-not-reserved-event-queue", RabbitMQSettingsConst.StockNotReservedEventQueue);
        Assert.Equal("stock-order-created-queue", RabbitMQSettingsConst.StockOrderCreatedEventQueue);
        Assert.Equal("order-payment-completed-queue", RabbitMQSettingsConst.OrderPaymentCompletedEventQueue);
        Assert.Equal("order-payment-failed-queue", RabbitMQSettingsConst.OrderPaymentFailedEventQueue);
        Assert.Equal("order-saga-queue", RabbitMQSettingsConst.OrderSaga);
        Assert.Equal("payment-stock-reserved-request-queue", RabbitMQSettingsConst.PaymentStockReservedRequestQueueName);
    }
}
