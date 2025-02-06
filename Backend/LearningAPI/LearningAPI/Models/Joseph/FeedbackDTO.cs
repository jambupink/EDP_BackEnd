using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearningAPI.Models.Joseph
{
    public class FeedbackDTO
    {
        public int Id { get; set; }
        public int Rating { get; set; }

        
        public string? FeedbackContent { get; set; }


        
        public DateTime CreatedAt { get; set; }
        
        public DateTime UpdatedAt { get; set; }
    }
}
