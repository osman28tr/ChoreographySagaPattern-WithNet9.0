using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.Dtos;
using Order.API.Models;
using Shared.Events;
using Shared.Messages;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        public OrdersController(AppDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(OrderCreateDto orderCreateDto)
        {
            var newOrder = new API.Models.Order
            {
                UserId = orderCreateDto.BuyerId,
                Address = new API.Models.Address
                {
                    Province = orderCreateDto.Address.Province,
                    District = orderCreateDto.Address.District,
                    Line = orderCreateDto.Address.Line
                },
                Status = OrderStatus.Suspend,
                TotalPrice = orderCreateDto.OrderItems.Sum(item => item.Price * item.Count),
                CreatedDate = DateTime.Now,
                FailMessage = string.Empty,
				Items = orderCreateDto.OrderItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId,
                    Price = item.Price,
                    Count = item.Count
                }).ToList()
            };

            _context.Orders.Add(newOrder);            
            await _context.SaveChangesAsync();
                       
            OrderCreatedEvent orderCreatedEvent = new()
            {
                BuyerId = newOrder.UserId,
                OrderId = newOrder.Id,
                PaymentMessage = new PaymentMessage
                {
                    TotalPrice = newOrder.TotalPrice,
                    CardName = orderCreateDto.Payment.CardName,
                    CardNumber = orderCreateDto.Payment.CardNumber,
                    Expiration = orderCreateDto.Payment.Expiration,
                    CVV = orderCreateDto.Payment.CVV
                },
                OrderItems = newOrder.Items.Select(item => new OrderItemMessage
                {
                    ProductId = item.ProductId,
                    Count = item.Count,
                }).ToList()
            };            

            await _publishEndpoint.Publish(orderCreatedEvent); //ilgili exchange'e subscribe olan kuyruk veya mikroservis yoksa boşa gider.

			return Ok("Order created successfully.");
		}
	}
}
