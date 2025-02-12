using LearningAPI.Models.Latiff;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


    public class Feedback
    {
        //PK
        public int Id { get; set; }
        public int Rating { get; set; }

        [MinLength(3), MaxLength(500)]
        public string? FeedbackContent { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; }

        //FK

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }

