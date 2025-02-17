using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Permissions;
using LearningAPI.Models.Latiff;

namespace LearningAPI.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required, MaxLength(100)]
        public string Comments { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(2,1)")]
        public decimal Rating { get; set; } // Rating to one decimal place

        [Required]
        [Column(TypeName = "datetime")]
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow; // Default to current time

        // Foreign Keys
        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}