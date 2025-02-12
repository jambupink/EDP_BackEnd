namespace LearningAPI.Models
{
    public class VariantDTO
    {
        public int? VariantId { get; set; } //Nullable for new varaiants
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}