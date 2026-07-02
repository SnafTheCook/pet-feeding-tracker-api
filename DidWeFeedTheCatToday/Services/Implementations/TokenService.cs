using DidWeFeedTheCatToday.Configuration;
using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.DTOs.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DidWeFeedTheCatToday.Services.Implementations
{
    public class TokenService(
        AppDbContext context,
        IOptions<AppSettings> options,
        IRequestContext requestContext,
        ILogger<TokenService> logger) : ITokenService
    {
        private readonly AppSettings _settings = options.Value;

        public async Task<TokenResponseDTO> CreateTokenResponse(User user)
        {
            var accessToken = CreateToken(user);
            var refreshToken = await GenerateAndSaveRefreshTokenAsync(user);

            return new TokenResponseDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenId = user.RefreshTokenId!.Value,
                UserId = user.Id
            };
        }
        public async Task<User?> ValidateRefreshTokenAsync(Guid userId, Guid refreshTokenId, string refreshToken)
        {
            var user = await context.Users.FindAsync(userId);

            if (user == null || user.RefreshTokenHash != HashToken(refreshToken) || user.RefreshTokenExpiryDate <= DateTime.UtcNow)
                return null;

            if (user.RefreshTokenId != refreshTokenId)
            {
                logger.LogWarning("Refresh token reuse detected! User: {0}, IP: {1}", user.Id, requestContext.IpAddress);
                return null;
            }

            return user;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Token));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            RandomNumberGenerator.Create().GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();

            user.RefreshTokenId = Guid.NewGuid();
            user.RefreshTokenHash = HashToken(refreshToken);
            user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(1);

            user.RefreshTokenCreatedByIp = requestContext.IpAddress;
            user.RefreshTokenCreatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return refreshToken;
        }

        private static string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
