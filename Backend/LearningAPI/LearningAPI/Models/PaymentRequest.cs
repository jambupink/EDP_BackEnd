using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
	public class PaymentRequest
	{
		public int OrderId { get; set; }
		public string PaymentMethod { get; set; }
		public string CustomerName { get; set; }
		public string Cvc { get; set; }
		public decimal Amount { get; set; }
	}

}
