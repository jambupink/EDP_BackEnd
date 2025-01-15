using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
	public class AddItemsToCartRequest
	{
		public int CartId { get; set; } // The Cart ID that will be shared for all items
		public List<Cart> CartItems { get; set; } // List of items to be added to the cart
	}

}
