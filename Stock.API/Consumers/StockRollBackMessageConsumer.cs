using MassTransit;
using Shared.Events;
using Shared.Messages.Abstract;
using Stock.API.Models;

namespace Stock.API.Consumers
{
	public class StockRollBackMessageConsumer : IConsumer<IStockRollBackMessage>
	{
		private readonly AppDbContext _context;
		private readonly ILogger<StockRollBackMessageConsumer> _logger;
		public StockRollBackMessageConsumer(AppDbContext context, ILogger<StockRollBackMessageConsumer> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task Consume(ConsumeContext<IStockRollBackMessage> context)
		{
			foreach (var item in context.Message.OrderItems)
			{
				var stock = await _context.Stocks.FindAsync(item.ProductId);
				if (stock != null)
				{
					stock.Count += item.Count;
				}
			}
			_logger.LogInformation($"Stock was incrumented");
			await _context.SaveChangesAsync();
		}
	}
}
