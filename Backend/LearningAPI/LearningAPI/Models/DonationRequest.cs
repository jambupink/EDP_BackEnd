namespace LearningAPI.Models
{
	public class DonationRequest
	{

		public string Title { get; set; } = string.Empty;

		public string Description { get; set; } = string.Empty;

		public String? DonationDateTime { get; set; }

		public string? ImageFile { get; set; }

		public string Condition { get; set; } = string.Empty;

	}
}