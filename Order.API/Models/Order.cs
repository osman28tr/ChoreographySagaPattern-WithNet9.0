namespace Order.API.Models
{
	public class Order
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public decimal TotalPrice { get; set; }
		public DateTime CreatedDate { get; set; }
		public Address Address { get; set; }
		public List<OrderItem> Items { get; set; }
		public OrderStatus Status { get; set; }
		public string? FailMessage { get; set; }
	}
	public enum OrderStatus
	{
		Suspend,
		Complete,
		Fail
	}
}
