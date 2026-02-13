using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DidWeFeedTheCatToday.Shared.DTOs.Auth;

namespace DidWeFeedTheCatToday.Services.Implementations
{
    public class AuthService(
        AppDbContext context, 
        IConfiguration configuration, 
        IRequestContext requestContext, 
        ILogger<AuthService> logger
        ) : IAuthServices
    {
        public async Task<TokenResponseDTO?> LoginAsync(UserDTO request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (user == null)
                return null;

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
                return null;

            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDTO> CreateTokenResponse(User user)
        {
            return new TokenResponseDTO
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user),
            };
        }

        public async Task<RegisterResponseDTO?> RegisterAsync(UserDTO request)
        {
            if (await context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            {
                return null;
            }

            var user = new User();
            var hashedPassword = new PasswordHasher<User>().HashPassword(user, request.Password);

            user.Username = request.Username;
            user.PasswordHash = hashedPassword;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var response = new RegisterResponseDTO()
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            };

            return response;
        }

        public async Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshTokenId, request.RefreshToken);

            return user == null ? null : await CreateTokenResponse(user);
        }

        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, Guid refreshTokenId, string refreshToken)
        {
            var user = await context.Users.FindAsync(userId);

            if (user == null 
                || user.RefreshTokenHash != HashToken(refreshToken) 
                || user.RefreshTokenExpiryDate <= DateTime.UtcNow)
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


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
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
