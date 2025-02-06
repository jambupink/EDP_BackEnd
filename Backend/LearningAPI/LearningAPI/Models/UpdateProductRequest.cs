using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
	public class UpdateProductRequest
	{
		[MinLength(3), MaxLength(100)]
		public string? ProductName { get; set; }

		[MinLength(3), MaxLength(300)]
		public string? Description { get; set; }

		[MaxLength(20)]
		public string? ImageFile { get; set; }

		public bool? IsArchived { get; set; } = false;
	}
}