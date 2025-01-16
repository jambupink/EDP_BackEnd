using System.Text.Json.Serialization;

namespace LearningAPI.Models
{
	public class Payment
	{
		public int PaymentId { get; set; }
		public int OrderId { get; set; }

		public int UserId { get; set; }
		public string PaymentMethod { get; set; }
		public string CustomerName { get; set; }
		public string Cvc { get; set; }
		public string CardNo { get; set; }
		public decimal Amount { get; set; }
		public DateTime PaymentDate { get; set; }
		public string Status { get; set; } = "Pending";

		[JsonIgnore]
		public Order Order { get; set; }
	}

}
