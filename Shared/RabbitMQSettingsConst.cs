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
        public const string StockNotReservedEventQueue = "stock-not-reserved-event-queue";
        public const string StockOrderCreatedEventQueue = "stock-order-created-queue";
        public const string OrderPaymentCompletedEventQueue = "order-payment-completed-queue";
        public const string OrderPaymentFailedEventQueue = "order-payment-failed-queue";
        public const string StockPaymentFailedEventQueue = "stock-payment-failed-queue";
    }
}
