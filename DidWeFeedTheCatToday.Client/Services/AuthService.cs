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
                await js.InvokeVoidAsync("localStorage.setItem", "authToken", result.Data.AccessToken);
                await js.InvokeVoidAsync("localStorage.setItem", "refreshToken", result.Data.RefreshToken);
                return true;
            }

            return false;
        }

        public async Task Logout()
        {
            await js.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await js.InvokeVoidAsync("localStorage.removeItem", "refreshToken");

            ((CustomAuthStateProvider)authProvider).NotifyUserLogout();
        }
    }
}
