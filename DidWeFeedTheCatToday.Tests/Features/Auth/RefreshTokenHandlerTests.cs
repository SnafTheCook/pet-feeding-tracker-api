using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Features.Auth.Commands;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.DTOs.Auth;
using FluentAssertions;
using Moq;

namespace DidWeFeedTheCatToday.Tests.Features.Auth
{
    public class RefreshTokenHandlerTests
    {
        private readonly Mock<ITokenService> _mockTokenService = new();
        private readonly RefreshTokenHandler _handler;

        public RefreshTokenHandlerTests()
        {
            _handler = new RefreshTokenHandler(_mockTokenService.Object);
        }

        [Fact]
        public async Task Handle_WithValidToken_ReturnsNewTokens()
        {
            var userId = Guid.NewGuid();
            var tokenId = Guid.NewGuid();
            var user = new User { Id = userId, Username = "testuser" };
            var request = new RefreshTokenRequestDTO
            {
                UserId = userId,
                RefreshTokenId = tokenId,
                RefreshToken = "old-refresh-token"
            };

            var newTokens = new TokenResponseDTO
            {
                AccessToken = "new-access",
                RefreshToken = "new-refresh",
                RefreshTokenId = Guid.NewGuid(),
                UserId = userId
            };

            _mockTokenService.Setup(s => s.ValidateRefreshTokenAsync(userId, tokenId, "old-refresh-token"))
                .ReturnsAsync(user);

            _mockTokenService.Setup(s => s.CreateTokenResponse(user))
                .ReturnsAsync(newTokens);

            var cmd = new RefreshTokenCommand(request);

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Success.Should().BeTrue();
            result.Data!.AccessToken.Should().Be("new-access");
        }

        [Fact]
        public async Task Handle_WithInvalidToken_ReturnsFailure()
        {
            _mockTokenService.Setup(s => s.ValidateRefreshTokenAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var cmd = new RefreshTokenCommand(new RefreshTokenRequestDTO
            {
                RefreshToken = "bad-token",
                RefreshTokenId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            });

            var result = await _handler.Handle(cmd, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.Error.Should().Be("Invalid or expired token.");
        }
    }
}
