using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Abstract
{
	public interface IOrderRequestFailedEvent
	{
		public int OrderId { get; set; }
		public string Reason { get; set; }
	}
}
