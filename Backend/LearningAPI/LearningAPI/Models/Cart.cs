namespace LearningAPI.Models
{
	public class Cart
	{
		public int CartId { get; set; }
		public int ProductId { get; set; }
		public int Quantity { get; set; }
		public string ProductName { get; set; }
		public string Size { get; set; }
		public int Price { get; set; }

		// Foreign key property
		public int UserId { get; set; }


	}






}
