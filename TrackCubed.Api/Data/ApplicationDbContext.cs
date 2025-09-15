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
        public DbSet<ItemType> ItemTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -- PRIMARY OWNERSHIP --
            // A User has many CubedItems. Deleting a user deletes what they created.
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.CubedItems)
                .WithOne(c => c.CreatedBy)
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Cascade);

            // A User owns their custom item types.
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.CustomItemTypes)
                .WithOne(it => it.User)
                .HasForeignKey(it => it.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // A CubedItem has one ItemType. An ItemType can be used by many CubedItems.
            // If you try to delete an ItemType that is in use, the operation should fail.
            modelBuilder.Entity<CubedItem>()
                .HasOne(ci => ci.ItemType)
                .WithMany()
                .HasForeignKey(ci => ci.ItemTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // -- THIS IS THE FIX --
            // A User has many Tags. But we cannot cascade this delete because it conflicts
            // with the User->CubedItem->CubedItemTag path.
            // We will handle deleting tags manually in our code.
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Tags)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Use RESTRICT to break the cycle.


            // Manually configure the join table to ensure its cascades are correct.
            modelBuilder.Entity<CubedItem>()
                .HasMany(c => c.Tags)
                .WithMany(t => t.CubedItems);

            // --- INDEXES & SEED DATA (Unchanged and Correct) ---
            modelBuilder.Entity<Tag>().HasIndex(t => new { t.UserId, t.Name }).IsUnique();
            modelBuilder.Entity<ItemType>().HasIndex(it => new { it.UserId, it.Name }).IsUnique().HasFilter("[UserId] IS NOT NULL"); // Index for custom types only

            // -- SEED DATA --
            modelBuilder.Entity<ItemType>().HasData(
                    new ItemType { Id = 1, Name = "Link", UserId = null },
                    new ItemType { Id = 2, Name = "Image", UserId = null },
                    new ItemType { Id = 3, Name = "Song", UserId = null },
                    new ItemType { Id = 4, Name = "Video", UserId = null },
                    new ItemType { Id = 5, Name = "Journal Entry", UserId = null },
                    new ItemType { Id = 6, Name = "Document", UserId = null },
                    new ItemType { Id = 7, Name = "Other", UserId = null }
            );
        }
    }
}
