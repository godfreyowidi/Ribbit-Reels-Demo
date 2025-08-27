using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using DotNetEnv;

namespace RibbitReels.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../RibbitReels.Api");

            var envPath = Path.Combine(basePath, ".env");
            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                Console.WriteLine($"Loaded .env from: {envPath}");
            }
            else
            {
                Console.WriteLine($"No .env found at: {envPath}. Will rely on environment variables.");
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("DefaultConnection not set in either .env or environment variables.");
            }

            if (Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true")
            {
                connectionString = connectionString.Replace("localhost", "sqlserver");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
