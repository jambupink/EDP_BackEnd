using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
	public class UpdateStockRequest
	{
		public int VariantId { get; set; }
		public int Stock { get; set; }
	}
}