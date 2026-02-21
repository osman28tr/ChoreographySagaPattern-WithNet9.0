using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateInstance : SagaStateMachineInstance //Order'ın state'ini ve gerekli diğer bilgilerini tutacak tabloya karşılık gelen sınıf
    { 
        public Guid CorrelationId { get; set; } // Her bir order state'e karşılık üretilecek id
        public string CurrentState { get; set; }
        public string BuyerId { get; set; }
        public int OrderId { get; set; }
        public DateTime CreatedDate { get; set; }
		public string CardName { get; set; }
		public string CardNumber { get; set; }
		public string Expiration { get; set; }
		public string CVV { get; set; }
		public decimal TotalPrice { get; set; }        
    }
}
