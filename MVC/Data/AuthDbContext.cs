using Microsoft.EntityFrameworkCore;
using ABCRetailers.Models;

namespace ABCRetailers.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Cart> Cart => Set<Cart>();
        // Remove: public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity (has Azure Table properties with [NotMapped])
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                // Only ignore properties that actually exist and are [NotMapped]
                entity.Ignore(u => u.PartitionKey);
                entity.Ignore(u => u.RowKey);
                entity.Ignore(u => u.Timestamp);
                entity.Ignore(u => u.ETag);
            });

            // Configure Cart entity (does NOT have Azure Table properties)
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(c => c.Id); // Make sure Cart has an Id property
                // No Ignore calls needed since Cart doesn't have Azure Table properties
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}