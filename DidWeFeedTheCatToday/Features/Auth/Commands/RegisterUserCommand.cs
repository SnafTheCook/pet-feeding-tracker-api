using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Features.Auth.Commands
{
    public record RegisterUserCommand(UserDTO UserDto) : IRequest<ApiResponse<RegisterResponseDTO>>;

    public class RegisterUserHandler(AppDbContext context) : IRequestHandler<RegisterUserCommand, ApiResponse<RegisterResponseDTO>>
    {
        public async Task<ApiResponse<RegisterResponseDTO>> Handle(RegisterUserCommand request, CancellationToken ct)
        {
            if (await context.Users.AnyAsync(u => u.Username.ToLower() == request.UserDto.Username.ToLower(), ct))
                return ApiResponse<RegisterResponseDTO>.Fail("Username is taken.");

            var user = new User { Username = request.UserDto.Username };
            user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.UserDto.Password);

            context.Users.Add(user);
            await context.SaveChangesAsync(ct);

            return ApiResponse<RegisterResponseDTO>.Ok(new RegisterResponseDTO { Id = user.Id, Username = user.Username, Role = user.Role });
        }
    }
}
