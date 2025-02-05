using LearningAPI.Models.Latiff;
using System;
using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
	public class ProductDTO
	{
		public int Id { get; set; }

		public string ProductName { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;
		public string? ImageFile { get; set; }

		public bool IsArchived { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime UpdatedAt { get; set; }

		public int UserId { get; set; }

		public UserBasicDTO? User { get; set; } // Assuming UserBasicDTO is a simplified DTO for User info.
	}
}