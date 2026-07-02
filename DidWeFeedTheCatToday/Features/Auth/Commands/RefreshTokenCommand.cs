using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Auth;
using MediatR;

namespace DidWeFeedTheCatToday.Features.Auth.Commands
{
    public record RefreshTokenCommand(RefreshTokenRequestDTO Request) : IRequest<ApiResponse<TokenResponseDTO>>;

    public class RefreshTokenHandler(ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, ApiResponse<TokenResponseDTO>>
    {
        public async Task<ApiResponse<TokenResponseDTO>> Handle(RefreshTokenCommand request, CancellationToken ct)
        {
            var user = await tokenService.ValidateRefreshTokenAsync(request.Request.UserId, request.Request.RefreshTokenId, request.Request.RefreshToken);

            if (user == null) return ApiResponse<TokenResponseDTO>.Fail("Invalid or expired token.");

            var tokens = await tokenService.CreateTokenResponse(user);
            return ApiResponse<TokenResponseDTO>.Ok(tokens);
        }
    }
}
