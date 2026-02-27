using MassTransit;
using Shared.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Abstract
{
	public interface IStockNotReservedEvent : CorrelatedBy<Guid>
	{
		public string Reason { get; set; }
	}
}
