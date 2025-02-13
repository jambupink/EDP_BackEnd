using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
    public class AddReviewRequest
    {
        [Required, MaxLength(100)]
        public string Comments { get; set; } = string.Empty;

        [Required]
        [Range(0.0, 5.0)] // Ensure rating is between 0 and 5
        public decimal Rating { get; set; }

        [Required]
        public int ProductId { get; set; } // FK for product
    }
}