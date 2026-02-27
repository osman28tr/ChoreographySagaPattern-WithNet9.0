using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Abstract
{
	public interface IOrderCreatedEvent : CorrelatedBy<Guid> //The correlateid implementation was created to indicate which
															 //state in the event's state machine database corresponds to
															 //which state.
	{
		public List<OrderItemMessage> OrderItems { get; set; }
	}
}
