namespace Order.API.Dtos
{
    public class OrderCreateDto
    {
        public string BuyerId { get; set; }
        public PaymentMessageDto Payment { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
        public AddressDto Address { get; set; }
	}
    public class PaymentMessageDto
    {
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }
        public decimal TotalPrice { get; set; }
	}
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
    public class AddressDto
    {
        public string Province { get; set; }
        public string District { get; set; }
        public string Line { get; set; }
	}
}
