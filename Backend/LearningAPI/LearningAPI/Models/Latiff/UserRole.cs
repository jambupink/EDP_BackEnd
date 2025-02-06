using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningAPI.Models.Latiff
{
    public class UserRole
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Description { get;set; } = string.Empty;
        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; }
        public List<User>? Users { get; set; }
    }
}
