using DidWeFeedTheCatToday.Configuration;
using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Implementations;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.DTOs.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DidWeFeedTheCatToday.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IRequestContext> _mockRequestContext = new();
        private readonly Mock<ILogger<AuthService>> _mockLogger = new();
        private readonly IOptions<AppSettings> _options;

        public AuthServiceTests()
        {
            _options = Options.Create(new AppSettings()
            {
                Token = "TestTokenVeryLongTokenForTestinghatNeedsToBe64CharactersLong!!!!",
                Issuer = "TestIssuer",
                Audience = "TestAudience"
            });
        }

        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task RegisterAsync_WhenUsernameExists_ReturnsNull()
        {
            using var context = GetDbContext();
            var service = new AuthService(context, _options, _mockRequestContext.Object, _mockLogger.Object);
            var request = new UserDTO { Username = "ExistingUser", Password = "Password123!" };

            context.Users.Add(new User { Username = "ExistingUser" });
            await context.SaveChangesAsync();

            var result = await service.RegisterAsync(request);

            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsToken()
        {
            using var db = GetDbContext();
            var password = "secretTestPassword123!";
            var username = "TestUser";
            var user = new User { Id = Guid.NewGuid(), Username = username };

            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var service = new AuthService(db, _options, _mockRequestContext.Object, _mockLogger.Object);
            var loginDto = new UserDTO { Username = username,  Password = password };

            var result = await service.LoginAsync(loginDto);

            result.Should().NotBeNull();
            result.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_WithWrongPassword_ReturnsNull()
        {
            using var db = GetDbContext();
            var username = "testUser";
            var user = new User { Id = Guid.NewGuid(), Username = username, PasswordHash = "someHash" };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var service = new AuthService(db, _options, _mockRequestContext.Object, _mockLogger.Object);
            var loginDto = new UserDTO { Username = username, Password = "wrongPassword" };

            var result = await service.LoginAsync(loginDto);

            result.Should().BeNull();
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenTokenIdMismatches_ReturnsNullAndLogsWarning()
        {
            var db = GetDbContext();
            var userId = Guid.NewGuid();
            var rawToken = "testtoken";
            var user = new User
            {
                Id = userId,
                RefreshTokenId = Guid.NewGuid(),
                RefreshTokenHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken))),
                RefreshTokenExpiryDate = DateTime.UtcNow.AddHours(1)
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var service = new AuthService(db, _options, _mockRequestContext.Object, _mockLogger.Object);

            var request = new RefreshTokenRequestDTO
            {
                UserId = userId,
                RefreshToken = rawToken,
                RefreshTokenId = Guid.NewGuid(),
            };

            var result = await service.RefreshTokenAsync(request);

            result.Should().BeNull();

            _mockLogger.Verify
                (
                    v => v.Log
                    (
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((x, t) => x.ToString()!.Contains("Refresh token reuse detected")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                    Times.Once
                );
        }
    }
}
