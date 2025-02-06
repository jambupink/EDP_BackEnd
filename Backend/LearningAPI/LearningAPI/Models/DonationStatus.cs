using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningAPI.Models
{
	public class DonationStatus
	{
		[Key]
		public int StatusId { get; set; } // Unique identifier

		[Required]
		public int DonationId { get; set; } // Foreign key to Donation

		[Required]
		public string Status { get; set; } = "Pending"; // Default to 'Pending'

		[Column(TypeName = "datetime")]
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // When status changed

		// Relationship (without modifying Donation.cs)
		[ForeignKey("DonationId")]
		public Donation? Donation { get; set; }
	}
}
