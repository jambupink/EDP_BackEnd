namespace LearningAPI.Models
{
	public class DonationDTO
	{
		public int Id { get; set; }

		public string Title { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public DateTime? DonationDateTime { get; set; }

		public string? ImageFile { get; set; }

		public int UserId { get; set; }

		public DateTime CreatedAt { get; set; }

		public DateTime UpdatedAt { get; set; }
		public string Condition { get; set; } = string.Empty;

	}
}