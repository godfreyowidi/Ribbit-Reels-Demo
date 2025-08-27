using BenchmarkDotNet.Attributes;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RibbitReels.Data;
using RibbitReels.Data.Models;

namespace RibbitReels.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(launchCount:1, warmupCount:2, iterationCount:5)]
public class EfVsDapperBenchmarks
{
    private AppDbContext _appDbContext = null!;
    private string _connectionString =
        "Server=localhost,1433;Database=RibbitReelsDb;User Id=sa;Password=Password123!;TrustServerCertificate=True;";

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(_connectionString)
            .Options;

        _appDbContext = new AppDbContext(options);
    }

    [Benchmark]
    public async Task<List<User>> EfCoreLinq()
    {
        return await _appDbContext.Users
            .Where(u => u.Role == UserRole.User)
            .ToListAsync();
    }

    [Benchmark]
    public async Task<List<User>> EfCoreRawSql()
    {
        return await _appDbContext.Users
            .FromSqlRaw("SELECT * FROM Users WHERE Role = 0")
            .ToListAsync();
    }

    [Benchmark]
    public async Task<List<User>> DapperQuery()
    {
        using var conn = new SqlConnection(_connectionString);
        return (await conn.QueryAsync<User>(
            "SELECT Id, Email, DisplayName, Role FROM Users WHERE Role = 0")).ToList();
    }

    [Benchmark]
    public async Task<List<User>> RawAdoNet()
    {
        var results = new List<User>();
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("SELECT Id, DisplayName, Role FROM Users WHERE Role = 0", conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new User
            {
                Id = reader.GetGuid(0),
                DisplayName = reader.GetString(1),
                Role = (UserRole)reader.GetInt32(2)
            });
        }

        return results;
    }
}