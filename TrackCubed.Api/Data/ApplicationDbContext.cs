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
        public DbSet<ApplicationUsers> ApplicationUsers { get; set; }

        // You will add other DbSet<T> properties here for your other models,
        // like CubedItem, Tag, etc., as your application grows.
    }
}
