using MassTransit;
using Shared.Abstract;

namespace Payment.API.Consumers
{
	public class StockReservedRequestPaymentConsumer : IConsumer<IStockReservedRequestPaymentEvent>
	{
		public Task Consume(ConsumeContext<IStockReservedRequestPaymentEvent> context)
		{
			throw new NotImplementedException();
		}
	}
}
