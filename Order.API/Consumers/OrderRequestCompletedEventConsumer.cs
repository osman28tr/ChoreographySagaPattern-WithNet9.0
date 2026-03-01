using MassTransit;
using Order.API.Models;
using Shared.Abstract;
using Shared.Events;

namespace Order.API.Consumers
{
	public class OrderRequestCompletedEventConsumer : IConsumer<IOrderRequestCompletedEvent>
	{
		private readonly ILogger<OrderRequestCompletedEventConsumer> _logger;
		private readonly AppDbContext _appDbContext;
		public OrderRequestCompletedEventConsumer(ILogger<OrderRequestCompletedEventConsumer> logger, AppDbContext appDbContext)
		{
			_logger = logger;
			_appDbContext = appDbContext;
		}

		public async Task Consume(ConsumeContext<IOrderRequestCompletedEvent> context)
		{
			var order = await _appDbContext.Orders.FindAsync(context.Message.OrderId);
			if (order != null)
			{
				order.Status = OrderStatus.Complete;
				await _appDbContext.SaveChangesAsync();
				_logger.LogInformation($"Order (Id={context.Message.OrderId}) status changed : {order.Status}");
			}
			else
			{
				_logger.LogError($"Order (Id={context.Message.OrderId}) not found");
			}
		}
	}
}
