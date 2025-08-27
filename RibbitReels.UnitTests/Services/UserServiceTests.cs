using Moq;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using RibbitReels.Data.Configs;
using RibbitReels.Data.Models;
using RibbitReels.Data.DTOs;
using RibbitReels.Data;
using System.Net;

namespace RibbitReels.UnitTests.Services;

public class UserServiceTests
{
    private readonly UserService _userService;
    private readonly AppDbContext _appDbContext;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;

    public UserServiceTests()
    {
        // mock google auth config
        var googleConfig = Options.Create(new GoogleAuthConfiguration
        {
            ClientId = "dummy",
            ClientSecret = "dummy"
        });

        // use in-memory EF Core DB
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _appDbContext = new AppDbContext(options);

        _passwordHasherMock = new Mock<IPasswordHasher<User>>();

        // dummy IConfiguration
        var configDict = new Dictionary<string, string>
        {
            { "Jwt:Key", "oFMxyP/bOJCKqnPseJtc7bdlJhzcy+nBDCmEA5g8gFg=" },
            { "Jwt:Issuer", "RibbitUnitTestIssuer" },
            {"Jwt:Audience", "RibbitUnitTestAudience"}
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict.Select(kvp => new KeyValuePair<string, string?>(kvp.Key, kvp.Value)))
            .Build();

        _userService = new UserService(googleConfig, _appDbContext, _passwordHasherMock.Object, configuration);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            DisplayName = "Test User",
            PasswordHash = "hashed-password"
        };

        _appDbContext.Users.Add(testUser);
        await _appDbContext.SaveChangesAsync();

        _passwordHasherMock
            .Setup(ph => ph.VerifyHashedPassword(testUser, "hashed-password", "correct-password"))
            .Returns(PasswordVerificationResult.Success);

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "correct-password",
        };

        // Act
        var result = await _userService.LoginAsync(request);

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(testUser.Email, result.Value.Email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var testUser = new User
        {
            Email = "user@example.com",
            PasswordHash = "hashed-password",
            DisplayName = "Test User",

        };
        _appDbContext.Users.Add(testUser);
        await _appDbContext.SaveChangesAsync();

        _passwordHasherMock
            .Setup(ph => ph.VerifyHashedPassword(testUser, "hashed-password", "wrong-password"))
            .Returns(PasswordVerificationResult.Failed);

        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "wrong-password"
        };

        // Act
        var result = await _userService.LoginAsync(request);

        // Assert
        Assert.False(result.IsSuccessful);
        Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "any"
        };

        var result = await _userService.LoginAsync(request);

        Assert.False(result.IsSuccessful);
        Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
    }
}
