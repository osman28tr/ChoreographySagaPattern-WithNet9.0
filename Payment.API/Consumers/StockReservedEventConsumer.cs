using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer : IConsumer<StockReservedEvent>
    {
        private readonly ILogger<StockReservedEventConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public StockReservedEventConsumer(ILogger<StockReservedEventConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            var balance = 5000m;
            if (balance > context.Message.Payment.TotalPrice)
            {
                _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was with drawn from card for userid={context.Message.BuyerId}");
                await _publishEndpoint.Publish(new PaymentCompletedEvent { OrderId = context.Message.OrderId, BuyerId = context.Message.BuyerId });
                return;
            }
            _logger.LogInformation($"{context.Message.Payment.TotalPrice} TL was with not drawn from card for userid={context.Message.BuyerId}");
            await _publishEndpoint.Publish(new PaymentFailedEvent
			{
                OrderId = context.Message.OrderId,
                BuyerId = context.Message.BuyerId,
                Message = "payment process was failed",
                OrderItems = context.Message.OrderItems
            });
        }
    }
}
