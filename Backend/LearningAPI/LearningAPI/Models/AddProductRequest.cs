using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
	public class AddProductRequest
	{
		[Required, MinLength(3), MaxLength(100)]
		public string ProductName { get; set; } = string.Empty;

		[Required, MinLength(3), MaxLength(300)]
		public string Description { get; set; } = string.Empty;

		[MaxLength(20)]
		public string? ImageFile { get; set; }

		public bool IsArchived { get; set; } = false; // Defaulting to not archived.
	}
}