using System.ComponentModel.DataAnnotations.Schema;
using LearningAPI.Models.Latiff;

namespace LearningAPI.Models
{
	public class Cart
	{
		public int CartId { get; set; }
		public int ProductId { get; set; }
		public Product? Product { get; set; }
		public int Quantity { get; set; }
		//public string ProductName { get; set; }
		//public string Size { get; set; }
		//public int Price { get; set; }

		// Foreign key property
		public int UserId { get; set; }
		public User? User { get; set; }
		public int VariantId { get; set; }
		public Variant? Variant { get; set; }


	}






}
