using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.DTOs.Auth;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Implementations;
using DidWeFeedTheCatToday.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidWeFeedTheCatToday.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfig = new();
        private readonly Mock<IRequestContext> _mockRequestContext = new();
        private readonly Mock<ILogger<AuthService>> _mockLogger = new();

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
            var service = new AuthService(context, _mockConfig.Object, _mockRequestContext.Object, _mockLogger.Object);
            var request = new UserDTO { Username = "ExistingUser", Password = "Password123!" };

            context.Users.Add(new User { Username = "ExistingUser" });
            await context.SaveChangesAsync();

            var result = await service.RegisterAsync(request);

            result.Should().BeNull();
        }
    }
}
