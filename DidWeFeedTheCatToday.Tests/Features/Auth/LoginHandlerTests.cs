using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Features.Auth.Commands;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.DTOs.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DidWeFeedTheCatToday.Tests.Features.Auth
{
    public class LoginHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<ITokenService> _mockTokenService = new();
        private readonly LoginHandler _handler;

        public LoginHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _context = new AppDbContext(options);
            _handler = new LoginHandler(_context, _mockTokenService.Object);
        }

        [Fact]
        public async Task Handle_WithValidCredentials_ReturnsTokens()
        {
            var user = new Entities.User { Username = "testuser" };
            user.PasswordHash = new PasswordHasher<Entities.User>().HashPassword(user, "CorrectPassword!");
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockTokenService.Setup(s => s.CreateTokenResponse(It.IsAny<Entities.User>()))
                .ReturnsAsync(new TokenResponseDTO { AccessToken = "abc", RefreshToken = "123", RefreshTokenId = Guid.NewGuid(), UserId = user.Id });

            var cmd = new LoginCommand(new UserDTO { Username = "testuser", Password = "CorrectPassword!" });

            var result = await _handler.Handle(cmd, default);

            result.Success.Should().BeTrue();
            result.Data!.AccessToken.Should().Be("abc");
        }

        public void Dispose() => _context.Dispose();
    }
}
