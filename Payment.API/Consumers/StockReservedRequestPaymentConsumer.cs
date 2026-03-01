using MassTransit;
using Shared.Abstract;
using Shared.Events;

namespace Payment.API.Consumers
{
	public class StockReservedRequestPaymentConsumer : IConsumer<IStockReservedRequestPaymentEvent>
	{
		private readonly ILogger<StockReservedRequestPaymentConsumer> _logger;
		private readonly IPublishEndpoint _publishEndpoint;

		public StockReservedRequestPaymentConsumer(ILogger<StockReservedRequestPaymentConsumer> logger, IPublishEndpoint publishEndpoint)
		{
			_logger = logger;
			_publishEndpoint = publishEndpoint;
		}

		public async Task Consume(ConsumeContext<IStockReservedRequestPaymentEvent> context)
		{
			var balance = 5000m;
			if (balance > context.Message.PaymentMessage.TotalPrice)
			{
				_logger.LogInformation($"{context.Message.PaymentMessage.TotalPrice} TL was with drawn from card for userid={context.Message.BuyerId}");
				await _publishEndpoint.Publish(new PaymentCompletedEvent(context.Message.CorrelationId));
				return;
			}
			_logger.LogInformation($"{context.Message.PaymentMessage.TotalPrice} TL was with not drawn from card for userid={context.Message.BuyerId}");
			await _publishEndpoint.Publish(new PaymentFailedEvent(context.Message.CorrelationId)
			{				
				Reason = "payment process was failed",
				OrderItems = context.Message.OrderItems
			});
		}
	}
}
