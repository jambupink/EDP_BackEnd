namespace LearningAPI.Models
{
	public class Order
	{
		public int OrderId { get; set; }
		public int UserId { get; set; }
		public DateTime OrderDate { get; set; }
		public string OrderStatus { get; set; } = "Pending";
		public List<OrderItem> OrderItems { get; set; }
		public decimal TotalPrice => OrderItems?.Sum(item => item.Price * item.Quantity) ?? 0;
	}

}
