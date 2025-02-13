using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
    public class UpdateReviewRequest
    {
        [Required, MaxLength(100)]
        public string Comments { get; set; } = string.Empty;

        [Required]
        [Range(0.0, 5.0)] // Ensure rating is between 0.0 - 5.0
        public decimal Rating { get; set; }
    }
}