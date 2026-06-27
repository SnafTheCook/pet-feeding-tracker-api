using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Shared.DTOs.Auth;

namespace DidWeFeedTheCatToday.Services.Interfaces
{
    public interface ITokenService
    {
        Task<TokenResponseDTO> CreateTokenResponse(User user);
        Task<User?> ValidateRefreshTokenAsync(Guid userId, Guid refreshTokenId, string refreshToken);
    }
}
