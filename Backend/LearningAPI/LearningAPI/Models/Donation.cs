﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace LearningAPI.Models
{
	public class Donation
	{
		public int Id { get; set; }

		[Required, MinLength(3), MaxLength(100)]
		public string Title { get; set; } = string.Empty;

		[Required, MinLength(3), MaxLength(500)]
		public string Description { get; set; } = string.Empty;

		[MaxLength(20)]
		public string? ImageFile { get; set; }

		[Required]
		[Column(TypeName = "datetime")]
		public DateTime? DonationDateTime { get; set; }

		[Column(TypeName = "datetime")]
		public DateTime CreatedAt { get; set; }

		[Column(TypeName = "datetime")]
		public DateTime UpdatedAt { get; set; }

		[Required]
		public string Condition { get; set; } = string.Empty;


		// Foreign key property
		public int UserId { get; set; }
	}
}