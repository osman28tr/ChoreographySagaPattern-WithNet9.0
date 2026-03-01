using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Order.API;
using Order.API.Controllers;
using Order.API.Dtos;
using Shared;
using Shared.Abstract;

namespace SagaPattern.Tests;

/// <summary>
/// OrdersController'ın birim testleri.
/// </summary>
public class OrdersControllerTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<ISendEndpointProvider> _sendEndpointProviderMock;
    private readonly Mock<ISendEndpoint> _sendEndpointMock;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new AppDbContext(options);

        _sendEndpointMock = new Mock<ISendEndpoint>();
        _sendEndpointMock
            .Setup(e => e.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sendEndpointProviderMock = new Mock<ISendEndpointProvider>();
        _sendEndpointProviderMock
            .Setup(p => p.GetSendEndpoint(It.IsAny<Uri>()))
            .ReturnsAsync(_sendEndpointMock.Object);

        _controller = new OrdersController(_dbContext, _sendEndpointProviderMock.Object);
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsOk()
    {
        var dto = BuildOrderCreateDto();

        var result = await _controller.CreateOrder(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Order created successfully.", okResult.Value);
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_PersistsOrderToDatabase()
    {
        var dto = BuildOrderCreateDto(buyerId: "user-1", totalItems: 2);

        await _controller.CreateOrder(dto);

        var orders = await _dbContext.Orders.Include(o => o.Items).ToListAsync();
        Assert.Single(orders);
        Assert.Equal("user-1", orders[0].UserId);
        Assert.Equal(2, orders[0].Items.Count);
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_CalculatesTotalPriceCorrectly()
    {
        var dto = new OrderCreateDto
        {
            BuyerId = "user-2",
            Address = new AddressDto { Province = "Istanbul", District = "Kadikoy", Line = "Moda Cd. No:1" },
            Payment = new PaymentMessageDto
            {
                CardName = "Ali Veli", CardNumber = "4111111111111111",
                Expiration = "12/27", CVV = "456", TotalPrice = 0
            },
            OrderItems = new List<OrderItemDto>
            {
                new() { ProductId = 1, Count = 2, Price = 100m },
                new() { ProductId = 2, Count = 3, Price = 50m  }
            }
        };

        await _controller.CreateOrder(dto);

        var order = await _dbContext.Orders.FirstOrDefaultAsync();
        Assert.NotNull(order);
        Assert.Equal(350m, order.TotalPrice); // 2*100 + 3*50 = 350
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_SendsEventToSagaQueue()
    {
        var dto = BuildOrderCreateDto();

        await _controller.CreateOrder(dto);

        _sendEndpointProviderMock.Verify(
            p => p.GetSendEndpoint(new Uri($"queue:{RabbitMQSettingsConst.OrderSaga}")),
            Times.Once);

        _sendEndpointMock.Verify(
            e => e.Send(It.IsAny<IOrderCreatedRequestEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_OrderStatusIsSuspend()
    {
        var dto = BuildOrderCreateDto();

        await _controller.CreateOrder(dto);

        var order = await _dbContext.Orders.FirstOrDefaultAsync();
        Assert.NotNull(order);
        Assert.Equal(Order.API.Models.OrderStatus.Suspend, order.Status);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private static OrderCreateDto BuildOrderCreateDto(string buyerId = "buyer-test", int totalItems = 1)
    {
        var items = Enumerable.Range(1, totalItems)
            .Select(i => new OrderItemDto { ProductId = i, Count = 2, Price = 75m })
            .ToList();

        return new OrderCreateDto
        {
            BuyerId = buyerId,
            Address = new AddressDto { Province = "Ankara", District = "Cankaya", Line = "Test Sk. No:5" },
            Payment = new PaymentMessageDto
            {
                CardName = "Test User", CardNumber = "4111111111111111",
                Expiration = "01/28", CVV = "789", TotalPrice = 150m
            },
            OrderItems = items
        };
    }
}
