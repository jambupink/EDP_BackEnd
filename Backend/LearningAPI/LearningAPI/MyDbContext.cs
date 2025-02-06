using LearningAPI.Models;
using LearningAPI.Models.Latiff;
using LearningAPI.Models.Joseph;
using Microsoft.EntityFrameworkCore;


namespace LearningAPI
{
    public class MyDbContext(IConfiguration configuration) : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string? connectionString = configuration.GetConnectionString("MyConnection");
            if (connectionString != null)
            {
                optionsBuilder.UseMySQL(connectionString);
            }
        }

        public required DbSet<Tutorial> Tutorials { get; set; }

        public required DbSet<User> Users { get; set; }
        public required DbSet<UserRole> UserRoles { get; set; }

        public required DbSet<Feedback> Feedbacks { get; set; }
    }
}