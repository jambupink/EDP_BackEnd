using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models
{
    public class UpdateProductRequest
    {
        [MinLength(3), MaxLength(100)]
        public string? ProductName { get; set; }

        [MinLength(3), MaxLength(300)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string? ImageFile { get; set; }

        public bool? IsArchived { get; set; } = false;

        [Required, MaxLength(30)]
        public string CategoryName { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string CategoryGender { get; set; } = string.Empty;
        public List<VariantDTO>? Variants { get; set; } //variant list
    }
}