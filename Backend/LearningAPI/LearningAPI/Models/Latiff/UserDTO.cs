namespace LearningAPI.Models.Latiff
{
    public class UserDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; } = false;
        public int Points { get; set; } = 0;
        public int UserRoleId { get; set; }
    }
}
