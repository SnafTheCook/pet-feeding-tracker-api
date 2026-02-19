using DidWeFeedTheCatToday.Client.Providers;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace DidWeFeedTheCatToday.Client.Services
{
    public class AuthService(HttpClient http, IJSRuntime js, AuthenticationStateProvider authProvider)
    {
        public async Task<bool> Login(UserDTO loginDto)
        {
            var response = await http.PostAsJsonAsync("api/auth/login", loginDto);
            
            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponseDTO>>();

            if (result != null && result.Success && result.Data != null)
            {
                await SaveTokens(result.Data);
                NotifyUserLogin(result.Data.AccessToken);
                return true;
            }

            return false;
        }

        public async Task Logout()
        {
            await js.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await js.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
            await js.InvokeVoidAsync("localStorage.removeItem", "refreshTokenId");
            await js.InvokeVoidAsync("localStorage.removeItem", "userId");

            ((CustomAuthStateProvider)authProvider).NotifyUserLogout();
        }

        public async Task<bool> RefreshToken()
        {
            var authToken = await js.InvokeAsync<string>("localStorage.getItem", "authToken");
            var refreshToken = await js.InvokeAsync<string>("localStorage.getItem", "refreshToken");
            var refreshTokenId = await js.InvokeAsync<string>("localStorage.getItem", "refreshTokenId");
            var userId = await js.InvokeAsync<string>("localStorage.getItem", "userId");

            if (string.IsNullOrEmpty(authToken) || 
                string.IsNullOrEmpty(refreshToken) || 
                string.IsNullOrEmpty(refreshTokenId) || 
                string.IsNullOrEmpty(userId))
                return false;

            var refreshTokenDto = new RefreshTokenRequestDTO
            {
                RefreshToken = refreshToken,
                RefreshTokenId = Guid.Parse(refreshTokenId),
                UserId = Guid.Parse(userId)
            };

            var response = await http.PostAsJsonAsync("api/auth/refreshToken", refreshTokenDto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<TokenResponseDTO>>();

                if (result != null && result.Success && result.Data != null)
                {
                    await SaveTokens(result.Data);
                    NotifyUserLogin(result.Data.AccessToken);
                    return true;
                }
            }

            await Logout();
            return false;
        }

        private async Task SaveTokens(TokenResponseDTO data)
        {
            await js.InvokeVoidAsync("localStorage.setItem", "authToken", data.AccessToken);
            await js.InvokeVoidAsync("localStorage.setItem", "refreshToken", data.RefreshToken);
            await js.InvokeVoidAsync("localStorage.setItem", "refreshTokenId", data.RefreshTokenId.ToString());
            await js.InvokeVoidAsync("localStorage.setItem", "userId", data.UserId.ToString());
        }

        private void NotifyUserLogin(string accessToken)
        {
            ((CustomAuthStateProvider)authProvider).NotifyUserLogin(accessToken);
        }
    }
}
