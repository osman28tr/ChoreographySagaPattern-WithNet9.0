using MassTransit;
using Shared.Abstract;

namespace Stock.API.Consumers
{
	public class OrderCreatedEventConsumer : IConsumer<IOrderCreatedEvent>
	{
		public Task Consume(ConsumeContext<IOrderCreatedEvent> context)
		{
			throw new NotImplementedException();
		}
	}
}
