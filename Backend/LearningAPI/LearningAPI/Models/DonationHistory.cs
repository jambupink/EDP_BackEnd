using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningAPI.Models
{
	public class DonationHistory
	{
		[Key]
		public int HistoryId { get; set; } // Primary Key

		[Required]
		public int DonationId { get; set; } // Foreign key to Donation

		[Required]
		public string ChangeDescription { get; set; } = string.Empty; // What changed

		[Column(TypeName = "datetime")]
		public DateTime ChangeDate { get; set; } = DateTime.UtcNow; // Timestamp of the change

		// Relationship (Linking Donation without modifying Donation.cs)
		[ForeignKey("DonationId")]
		public Donation? Donation { get; set; }
	}
}

