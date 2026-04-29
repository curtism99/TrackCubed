using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TrackCubed.Api.Data
{
    public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environment))
            {
                environment = "Development";
            }

            var basePath = Directory.GetCurrentDirectory();
            if (!File.Exists(Path.Combine(basePath, "appsettings.json")) &&
                Directory.Exists(Path.Combine(basePath, "TrackCubed.Api")))
            {
                basePath = Path.Combine(basePath, "TrackCubed.Api");
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var authMode = configuration["TrackCubed:Auth:Mode"];
            var useDevelopmentAuth = environment.Equals("Development", StringComparison.OrdinalIgnoreCase) &&
                authMode?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true;

            var connectionString = useDevelopmentAuth
                ? configuration["TrackCubed:LocalDevelopment:ConnectionString"]
                : configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("No database connection string is configured.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure();
            });

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
