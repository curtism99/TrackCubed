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
        public DbSet<SystemItemType> SystemItemTypes { get; set; }
        public DbSet<UserItemType> UserItemTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- RELATIONSHIPS ---
            // By convention, EF Core should figure out the relationships. The issue is purely with cascade deletes.
            // Let's explicitly state the one that is causing the problem.

            // An ApplicationUser has many CubedItems. When the user is deleted, delete their items.
            // This will cascade to the CubedItemTag table correctly.
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.CubedItems)
                .WithOne(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Cascade);

            // A User has many Tags. THIS is the conflicting cascade path. SQL Server doesn't
            // know if it should delete the CubedItemTag records because the User deleted the Tag,
            // or because the User deleted the CubedItem. We must disable one.
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Tags)
                .WithOne(t => t.User)
                .OnDelete(DeleteBehavior.Restrict); // This tells SQL "Do not delete a user if they still have tags"

            // A User has many CustomItemTypes. This is a simple relationship with no cycles.
            modelBuilder.Entity<ApplicationUser>()
               .HasMany(u => u.CustomItemTypes)
               .WithOne(uit => uit.User)
               .OnDelete(DeleteBehavior.Cascade);

            // Manually configure the join table
            modelBuilder.Entity<CubedItem>()
                .HasMany(c => c.Tags)
                .WithMany(t => t.CubedItems)
                .UsingEntity("CubedItemTag",
                    l => l.HasOne(typeof(Tag)).WithMany().HasForeignKey("TagsId").OnDelete(DeleteBehavior.Cascade),
                    r => r.HasOne(typeof(CubedItem)).WithMany().HasForeignKey("CubedItemsId").OnDelete(DeleteBehavior.NoAction),
                    j => j.HasKey("CubedItemsId", "TagsId"));

            // --- INDEX CONFIGURATIONS ---
            modelBuilder.Entity<Tag>().HasIndex(t => new { t.UserId, t.Name }).IsUnique();
            modelBuilder.Entity<UserItemType>().HasIndex(uit => new { uit.UserId, uit.Name }).IsUnique();

            // --- SEED DATA ---
            modelBuilder.Entity<SystemItemType>().HasData(
                new SystemItemType { Id = 1, Name = "Link" },
                new SystemItemType { Id = 2, Name = "Image" },
                new SystemItemType { Id = 3, Name = "Song" },
                new SystemItemType { Id = 4, Name = "Video" },
                new SystemItemType { Id = 5, Name = "Journal Entry" },
                new SystemItemType { Id = 6, Name = "Document" },
                new SystemItemType { Id = 7, Name = "Other" }
            );
            modelBuilder.Entity<Tag>().HasData(
               new { Id = 1, Name = "dummy", UserId = 1 }
            );
        }
    }
}
