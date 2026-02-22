using MassTransit;
using Shared.Abstract;
using Shared.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Models
{
	public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
	{
		public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }
		public State OrderCreated { get; private set; }
		public OrderStateMachine()
		{
			InstanceState(x => x.CurrentState); //Set the state to init.

			Event(() => OrderCreatedRequestEvent, y => y.CorrelateBy<int>(x => x.OrderId, z => z.Message.OrderId).SelectId(context => Guid.NewGuid())); //Compare the order ID in the incoming request with the order ID in the state instance table. If it exists, set its status to init; otherwise, create it.

			Initially(When(OrderCreatedRequestEvent).Then(context =>
			{
				context.Instance.BuyerId = context.Data.BuyerId;
				context.Instance.OrderId = context.Data.OrderId;
				context.Instance.CardName = context.Data.Payment.CardName;
				context.Instance.CardNumber = context.Data.Payment.CardNumber;
				context.Instance.CreatedDate = DateTime.Now;
				context.Instance.CVV = context.Data.Payment.CVV;
				context.Instance.Expiration = context.Data.Payment.Expiration;
				context.Instance.TotalPrice = context.Data.Payment.TotalPrice;
			})//When the order status is init, the order created request transition phase, then the block where the business code to be executed in the relevant transaction is written
				.Then(context => { Console.WriteLine($"OrderCreatedRequestEvent before : {context.Instance}"); })
				.TransitionTo(OrderCreated) //Set the state of the relevant order to OrderCreated
				.Then(context => { Console.WriteLine($"OrderCreatedRequestEvent after : {context.Instance}"); })
				);
		}
	}
}
