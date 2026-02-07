using MassTransit;
using Shared.Events;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
		private readonly AppDbContext _context;
		private readonly ILogger<PaymentFailedEventConsumer> _logger;
        public PaymentFailedEventConsumer(AppDbContext context, ILogger<PaymentFailedEventConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            foreach(var item in context.Message.OrderItems)
            {
                var stock = await _context.Stocks.FindAsync(item.ProductId);
                if (stock != null)
                {
                    stock.Count += item.Count;
                }
            }
            _logger.LogInformation($"Stock was incrument for order id = {context.Message.OrderId}");
            await _context.SaveChangesAsync();
        }
    }
}
