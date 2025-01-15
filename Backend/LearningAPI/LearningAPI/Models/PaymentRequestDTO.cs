namespace LearningAPI.Models
{
	public class PaymentRequestDto
	{
		public int OrderId { get; set; }
		public decimal Amount { get; set; }
		public required string PaymentMethod { get; set; } // e.g., Credit Card, Bank Transfer, etc.
	}


}
