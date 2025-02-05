using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LearningAPI.Models.Latiff
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(200), JsonIgnore]
        public string Password { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Gender { get; set; } = string.Empty;
        [MaxLength(20)]
        public string MobileNumber { get; set; } = string.Empty;
        [MaxLength(200)]
        public string Address { get; set; } = string.Empty;
        public int Points { get; set; } = 0;

        public bool IsEmailConfirmed { get; set; } = false;
        public string EmailConfirmationToken { get; set; } = string.Empty;
        public DateTime? EmailConfirmationTokenExpiry { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; }

        // Navigation property to represent the one-to-many relationship
        [JsonIgnore]
        public List<Tutorial>? Tutorials { get; set; }

        public int UserRoleId { get; set; }
        public UserRole? UserRole { get; set; }
    }
}