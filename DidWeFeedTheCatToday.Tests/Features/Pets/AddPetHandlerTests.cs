using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Data.Interceptors;
using DidWeFeedTheCatToday.Features.Pets;
using DidWeFeedTheCatToday.Features.Pets.Notifications;
using DidWeFeedTheCatToday.Mappers;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DidWeFeedTheCatToday.Tests.Features.Pets
{
    public class AddPetHandlerTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IPublisher> _mockPublisher = new();
        private readonly AddPetHandler _handler;
        private readonly PetMapper _mapper;

        public AddPetHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddInterceptors(new UpdateAuditableInterceptor()).Options;

            _context = new AppDbContext(options);
            _mapper = new PetMapper();
            _handler = new AddPetHandler(_context, _mockPublisher.Object, _mapper);
        }

        [Fact]
        public async Task Handle_WhenValid_SavesAndPublishes()
        {
            var cmd = new AddPetCommand(new CommandPetDTO { Name = "Meowstarion", Age = 3 });

            var result = await _handler.Handle(cmd, default);

            result.Id.Should().BeGreaterThan(0);
            _context.Pets.Should().Contain(p => p.Name == "Meowstarion");
            _mockPublisher.Verify(p => p.Publish(It.IsAny<PetChangedNotification>(), default), Times.Once);
        }
    }
}
