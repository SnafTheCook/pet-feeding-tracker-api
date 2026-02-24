using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace DidWeFeedTheCatToday.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController(IAuthServices authService) : ControllerBase
    {
        /// <summary>
        /// Handles user registration.
        /// </summary>
        /// <param name="request"><see cref="UserDTO"/> with username and password</param>
        /// <returns>API response containing status and new user's profile or and error message</returns>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<RegisterResponseDTO>>> Register(UserDTO request)
        {
            var result = await authService.RegisterAsync(request);

            if (result == null)
                return BadRequest(ApiResponse<RegisterResponseDTO>.Fail("Username is taken. Try different username."));

            return Ok(ApiResponse<RegisterResponseDTO>.Ok(result));
        }

        /// <summary>
        /// Handles user login.
        /// </summary>
        /// <param name="request"><see cref="UserDTO"/> with username and password</param>
        /// <returns>API response containing status with Access Token and Refresh Token</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<TokenResponseDTO>>> Login(UserDTO request)
        {
            var result = await authService.LoginAsync(request);

            if (result == null)
                return Unauthorized(ApiResponse<TokenResponseDTO>.Fail("Invalid login credentials. Try again."));

            return Ok(ApiResponse<TokenResponseDTO>.Ok(result));
        }

        /// <summary>
        /// Handles token refresh.
        /// </summary>
        /// <param name="request"><see cref="RefreshTokenRequestDTO"/> containing user's Refresh Token</param>
        /// <returns>API response containing status with Access Token and Refresh Token</returns>
        [HttpPost("refreshToken")]
        public async Task<ActionResult<ApiResponse<TokenResponseDTO>>> RefreshToken(RefreshTokenRequestDTO request)
        {
            var result = await authService.RefreshTokenAsync(request);

            if (result == null || result.RefreshToken == null || result.AccessToken == null)
                return Unauthorized(ApiResponse<TokenResponseDTO>.Fail("Invalid or expired refresh token."));

            return Ok(ApiResponse<TokenResponseDTO>.Ok(result));
        }
    }
}
