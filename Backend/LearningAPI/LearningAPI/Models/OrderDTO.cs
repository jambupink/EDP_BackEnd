namespace LearningAPI.Models
{
	public class OrderDTO
	{
		public int OrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public string OrderStatus { get; set; } = "Pending";
		public List<OrderItem> OrderItems { get; set; }
		public DateTime? DeliveryDate { get; set; }
	}

}
