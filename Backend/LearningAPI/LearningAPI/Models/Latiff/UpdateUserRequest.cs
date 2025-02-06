using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LearningAPI.Models.Latiff
{
    public class UpdateUserRequest
    {
        [MinLength(3), MaxLength(50)]
        // Regular expression to enforce name format
        [RegularExpression(@"^[a-zA-Z '-,.]+$", ErrorMessage = "Only allow letters, spaces and characters: ' - , .")]
        public string? Name { get; set; } 

        [EmailAddress, MaxLength(50)]
        public string? Email { get; set; } 

        [MinLength(8), MaxLength(50)]
        // Regular expression to enforce password complexity
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*[0-9]).{8,}$", ErrorMessage = "At least 1 letter and 1 number")]
        public string? Password { get; set; } 
        [MinLength(3), MaxLength(50)]
        public string? Gender { get; set; } 
        [MinLength(3), MaxLength(20)]
        public string? MobileNumber { get; set; } 
        [MinLength(3), MaxLength(200)]
        public string? Address { get; set; }
        public bool? IsEmailConfirmed { get; set; } 
        public int? Points { get; set; }
        public int? UserRoleId { get; set; }
    }
}
