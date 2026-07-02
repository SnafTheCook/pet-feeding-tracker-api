using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Features.Auth.Commands
{
    public record LoginCommand(UserDTO UserDto) : IRequest<ApiResponse<TokenResponseDTO>>;

    public class LoginHandler(AppDbContext context, ITokenService tokenService) : IRequestHandler<LoginCommand, ApiResponse<TokenResponseDTO>>
    {
        public async Task<ApiResponse<TokenResponseDTO>> Handle(LoginCommand request, CancellationToken ct)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.UserDto.Username.ToLower(), ct);

            if (user == null || new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.UserDto.Password) == PasswordVerificationResult.Failed)
                return ApiResponse<TokenResponseDTO>.Fail("Invalid credentials.");

            var tokens = await tokenService.CreateTokenResponse(user);
            return ApiResponse<TokenResponseDTO>.Ok(tokens);
        }
    }
}
