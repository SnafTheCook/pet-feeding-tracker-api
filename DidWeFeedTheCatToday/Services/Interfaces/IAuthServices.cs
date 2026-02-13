using DidWeFeedTheCatToday.Shared.DTOs.Auth;

namespace DidWeFeedTheCatToday.Services.Interfaces
{
    public interface IAuthServices
    {
        Task<RegisterResponseDTO?> RegisterAsync(UserDTO request);
        Task<TokenResponseDTO?> LoginAsync(UserDTO request);
        Task<TokenResponseDTO?> RefreshTokenAsync(RefreshTokenRequestDTO request);
    }
}
