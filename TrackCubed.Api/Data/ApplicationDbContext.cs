using Microsoft.EntityFrameworkCore;
using TrackCubed.Shared.Models;

namespace TrackCubed.Api.Data
{
    public class ApplicationDbContext : DbContext
    {


        // The constructor that takes DbContextOptions is essential for dependency injection.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // This DbSet property represents the "Users" table in your database.
        // EF Core will create a table named "ApplicationUsers" by default.
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        // You will add other DbSet<T> properties here for your other models,
        // like CubedItem, Tag, etc., as your application grows.

        public DbSet<CubedItem> CubedItems { get; set; }
        public DbSet<Tag> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the one-to-many relationship between User and CubedItem
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.CubedItems)
                .WithOne(c => c.CreatedBy)
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Cascade); // If a user is deleted, delete their items

            // Configure a unique index on Tag.Name to prevent duplicate tags
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.Name)
                .IsUnique();
        }
    }
}
