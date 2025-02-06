using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models.Joseph
{
    public class AddFeedbackRequest
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [MinLength(3, ErrorMessage = "Feedback content must be at least 3 characters.")]
        [MaxLength(500, ErrorMessage = "Feedback content must not exceed 500 characters.")]
        public string? FeedbackContent { get; set; }
    }
}
