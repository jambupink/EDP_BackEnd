using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LearningAPI.Models
{
	public class OrderItem
	{
		public int OrderItemId { get; set; }
		public int OrderId { get; set; }
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }
		public string Size { get; set; }

		[JsonIgnore]
		public Order Order { get; set; }
	}

}
