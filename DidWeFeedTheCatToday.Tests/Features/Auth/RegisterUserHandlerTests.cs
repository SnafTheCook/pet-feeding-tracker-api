using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Features.Auth.Commands;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Tests.Features.Auth
{

    public class RegisterUserHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly RegisterUserHandler _handler;

        public RegisterUserHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _context = new AppDbContext(options);
            _handler = new RegisterUserHandler(_context);
        }

        [Fact]
        public async Task Handle_WhenUsernameUnique_ReturnsSuccess()
        {
            var cmd = new RegisterUserCommand(new Shared.DTOs.Auth.UserDTO { Username = "NewUser", Password = "Password123!" });

            var result = await _handler.Handle(cmd, default);

            result.Success.Should().BeTrue();
            result.Data!.Username.Should().Be("NewUser");
            (await _context.Users.AnyAsync(u => u.Username == "NewUser")).Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WhenUsernameExists_ReturnsFailure()
        {
            _context.Users.Add(new Entities.User { Username = "existinguser" });
            await _context.SaveChangesAsync();

            var cmd = new RegisterUserCommand(new Shared.DTOs.Auth.UserDTO { Username = "ExistingUser", Password = "Password123!" });

            var result = await _handler.Handle(cmd, default);

            result.Success.Should().BeFalse();
            result.Error.Should().Be("Username is taken.");
        }

        public void Dispose() => _context.Dispose();
    }
}