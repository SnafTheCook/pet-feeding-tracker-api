using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Features.Pets.Commands;
using DidWeFeedTheCatToday.Features.Pets.Notifications;
using DidWeFeedTheCatToday.Mappers;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DidWeFeedTheCatToday.Tests.Features.Pets
{
    public class UpdatePetHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IPublisher> _mockPublisher = new();
        private readonly UpdatePetHandler _handler;
        private readonly PetMapper _petMapper;

        public UpdatePetHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _context = new AppDbContext(options);
            _petMapper = new PetMapper();
            _handler = new UpdatePetHandler(_context, _mockPublisher.Object, _petMapper);
        }

        [Fact]
        public async Task Handle_WhenPetExists_UpdatesDatabase()
        {
            var pet = new Pet { Name = "Old Name" };
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            var cmd = new UpdatePetCommand(pet.Id, new CommandPetDTO { Name = "New Name" });

            var result = await _handler.Handle(cmd, default);

            result.Success.Should().BeTrue();

            var updated = await _context.Pets.FindAsync(pet.Id);
            updated!.Name.Should().Be("New Name");

            _mockPublisher.Verify(p => p.Publish(It.IsAny<PetChangedNotification>(), default), Times.Once);
        }

        public void Dispose() => _context.Dispose();
    }
}
