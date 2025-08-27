using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RibbitReels.Data;
using RibbitReels.Services.Interfaces;
using RibbitReels.Services.Shared;

namespace RibbitReels.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>
{
    private readonly string _inMemoryDbName = "SharedTestDb";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");

        // fake JWT environment variables
        Environment.SetEnvironmentVariable("Jwt__Key", "oFMxyP/bOJCKqnPseJtc7bdlJhzcy+nBDCmEA5g8gFg=");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "RibbitIntegrationTestIssuer");
        Environment.SetEnvironmentVariable("Jwt__Audience", "RibbitIntegrationTestAudience");

        // fake connection string to prevent Program.cs from crashing
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            "Server=(localdb)\\mssqllocaldb;Database=FakeTestDb;Trusted_Connection=True;"
        );

        builder.ConfigureServices(services =>
        {
            // we remove existing DbContext registrations (SQL Server)
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();
            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            // add InMemory DbContext with fixed database name - shared declare globally 
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_inMemoryDbName)
                       .EnableSensitiveDataLogging());

            // we replace AzureBlobRepository with a fake implementation
            var azureDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAzureBlobRepository));
            if (azureDescriptor != null) services.Remove(azureDescriptor);
            services.AddSingleton<IAzureBlobRepository, FakeAzureBlobRepository>();

        });
    }
}

/// fake Azure Blob for testing
public class FakeAzureBlobRepository : IAzureBlobRepository
{
    public Task<OperationResult<string>> UploadVideoAsync(IFormFile file, string leafId)
        => Task.FromResult(OperationResult<string>.Success($"fake/path/{leafId}/{file.FileName}"));

    public Task<OperationResult<string>> GetVideoUrlAsync(string blobPath, int validMinutes = 60)
        => Task.FromResult(OperationResult<string>.Success($"https://fake.blob/{blobPath}"));
}
