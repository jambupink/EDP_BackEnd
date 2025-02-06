﻿using LearningAPI.Models;
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
		public required DbSet<Donation> Donations { get; set; }
		public required DbSet<DonationHistory> DonationHistories { get; set; }
		public required DbSet<DonationStatus> DonationStatuses { get; set; }
	}
}