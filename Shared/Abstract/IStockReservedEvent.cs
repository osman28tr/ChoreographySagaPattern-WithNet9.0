using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Abstract
{
	public interface IStockReservedEvent : CorrelatedBy<Guid>
	{
		public List<OrderItemMessage> OrderItems { get; set; }
	}
}
