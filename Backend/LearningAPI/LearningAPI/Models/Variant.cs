using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningAPI.Models
{
    public class Variant
    {
        [Key]
        public int VariantId { get; set; }

        [Required, MaxLength(30)]
        public string Color { get; set; } = string.Empty;

        [Required, MaxLength(5)]
        public string Size { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(6,2)")]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; }

        // foreign key to product
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}