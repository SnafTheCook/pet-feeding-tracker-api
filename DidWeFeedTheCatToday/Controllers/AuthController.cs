using Asp.Versioning;
using DidWeFeedTheCatToday.Features.Auth.Commands;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DidWeFeedTheCatToday.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    public class AuthController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Handles user registration.
        /// </summary>
        /// <param name="request"><see cref="UserDTO"/> with username and password</param>
        /// <returns>API response containing status and new user's profile or and error message</returns>
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<RegisterResponseDTO>>> Register(UserDTO request)
        {
            var result = await mediator.Send(new RegisterUserCommand(request));
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Handles user login.
        /// </summary>
        /// <param name="request"><see cref="UserDTO"/> with username and password</param>
        /// <returns>API response containing status with Access Token and Refresh Token</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<TokenResponseDTO>>> Login(UserDTO request)
        {
            var result = await mediator.Send(new LoginCommand(request));
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        /// <summary>
        /// Handles token refresh.
        /// </summary>
        /// <param name="request"><see cref="RefreshTokenRequestDTO"/> containing user's Refresh Token</param>
        /// <returns>API response containing status with Access Token and Refresh Token</returns>
        [HttpPost("refreshToken")]
        public async Task<ActionResult<ApiResponse<TokenResponseDTO>>> RefreshToken(RefreshTokenRequestDTO request)
        {
            var result = await mediator.Send(new RefreshTokenCommand(request));
            return result.Success ? Ok(result) : Unauthorized(result);
        }
    }
}
