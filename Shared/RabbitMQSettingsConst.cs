using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class RabbitMQSettingsConst
    {
        public const string StockReservedEventQueue = "stock-reserved-event-queue";
        public const string StockOrderCreatedEventQueue = "stock-order-created-queue";
    }
}
