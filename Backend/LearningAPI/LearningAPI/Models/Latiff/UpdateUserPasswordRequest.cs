using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models.Latiff
{
    public class UpdateUserPasswordRequest
    {
        [MinLength(8), MaxLength(50)]
        // Regular expression to enforce password complexity
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*[0-9]).{8,}$", ErrorMessage = "At least 1 letter and 1 number")]
        public string? Password { get; set; }
        [MinLength(8), MaxLength(50)]
        // Regular expression to enforce password complexity
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*[0-9]).{8,}$", ErrorMessage = "At least 1 letter and 1 number")]
        public string? NewPassword { get; set; }
    }
}
