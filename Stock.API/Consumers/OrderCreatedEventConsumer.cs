using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Abstract;
using Shared.Events;
using Stock.API.Models;

namespace Stock.API.Consumers
{
	public class OrderCreatedEventConsumer : IConsumer<IOrderCreatedEvent>
	{
		private readonly AppDbContext _context;
		private readonly ILogger<OrderCreatedEventConsumer> _logger;
		private readonly IPublishEndpoint _publishEndpoint;

		public OrderCreatedEventConsumer(AppDbContext context, ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
		{
			_context = context;
			_logger = logger;
			_publishEndpoint = publishEndpoint;
		}

		public async Task Consume(ConsumeContext<IOrderCreatedEvent> context)
		{
			var stockResult = new List<bool>();
			foreach (var item in context.Message.OrderItems)
			{
				stockResult.Add(await _context.Stocks.AnyAsync(x => x.ProductId == item.ProductId && x.Count > item.Count));
			}

			if (stockResult.Any(x => x != true))
			{
				await _publishEndpoint.Publish(new StockNotReservedEvent(context.Message.CorrelationId)
				{
					Reason = "Not enough stock"
				});
				return;
			}
			foreach (var item in context.Message.OrderItems)
			{
				var stock = await _context.Stocks.FirstOrDefaultAsync(x => x.ProductId == item.ProductId);
				if (stock != null)
				{
					stock.Count -= item.Count;
				}
			}
			var stockReservedEvent = new StockReservedEvent(context.Message.CorrelationId)
			{
				OrderItems = context.Message.OrderItems
			};

			await _publishEndpoint.Publish(stockReservedEvent);
			_logger.LogInformation($"Stock was reserved for Buyer Id : {context.Message.CorrelationId}");
		}
	}
}
