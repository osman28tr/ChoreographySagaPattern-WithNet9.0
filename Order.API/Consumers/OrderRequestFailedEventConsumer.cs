using MassTransit;
using Order.API.Models;
using Shared.Abstract;

namespace Order.API.Consumers
{
	public class OrderRequestFailedEventConsumer : IConsumer<IOrderRequestFailedEvent>
	{
		private readonly ILogger<OrderRequestFailedEventConsumer> _logger;
		private readonly AppDbContext _appDbContext;
		public OrderRequestFailedEventConsumer(ILogger<OrderRequestFailedEventConsumer> logger, AppDbContext appDbContext)
		{
			_logger = logger;
			_appDbContext = appDbContext;
		}
		public async Task Consume(ConsumeContext<IOrderRequestFailedEvent> context)
		{
			var order = await _appDbContext.Orders.FindAsync(context.Message.OrderId);
			if (order != null)
			{
				order.Status = OrderStatus.Fail;
				order.FailMessage = context.Message.Reason;
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
