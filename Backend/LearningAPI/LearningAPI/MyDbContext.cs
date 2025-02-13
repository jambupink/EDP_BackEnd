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



		public required DbSet<Cart> Carts { get; set; }
		public required DbSet<Order> Orders { get; set; }
		public required DbSet<OrderItem> OrderItems { get; set; }
		public required DbSet<Payment> Payments { get; set; }
    public required DbSet<User> Users { get; set; }
		public required DbSet<Donation> Donations { get; set; }
		public required DbSet<DonationHistory> DonationHistories { get; set; }
		public required DbSet<DonationStatus> DonationStatuses { get; set; }
    public required DbSet<UserRole> UserRoles { get; set; }
		public required DbSet<Product> Products { get; set; }
     public required DbSet<Feedback> Feedbacks { get; set; }
        public required DbSet<Variant> Variants { get; set; }
        public DbSet<Review> Reviews { get; set; }


    }

}