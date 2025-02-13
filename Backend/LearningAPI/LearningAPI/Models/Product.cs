using LearningAPI.Models.Latiff;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningAPI.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, MinLength(3), MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Required, MinLength(3), MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? ImageFile { get; set; }

        public bool? IsArchived { get; set; } = false;

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Required, MaxLength(30)]
        public string CategoryName { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string CategoryGender { get; set; } = string.Empty;

        // Foreign Keys
        public List<Review>? Reviews { get; set; } // Allow multiple reviews

        public List<Variant>? Variants { get; set; } //allow multiple variants
    }
}