namespace LearningAPI.DTOs
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }
        public string Comments { get; set; } = string.Empty;
        public decimal Rating { get; set; } 
        public DateTime ReviewDate { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty; // Include the username for display
    }
}