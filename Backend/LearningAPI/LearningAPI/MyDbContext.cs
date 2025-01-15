using LearningAPI.Models;
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

		public DbSet<Cart> Carts { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }
		public DbSet<Payment> Payments { get; set; }
	}
}