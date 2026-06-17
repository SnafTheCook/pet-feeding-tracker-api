using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Features.Pets.Commands;
using DidWeFeedTheCatToday.Features.Pets.Notifications;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidWeFeedTheCatToday.Tests.Features.Pets
{
    public class DeletePetHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IPublisher> _mockPublisher = new();
        private readonly DeletePetHandler _handler;

        public DeletePetHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _handler = new DeletePetHandler(_context, _mockPublisher.Object);
        }

        [Fact]
        public async Task Handle_WhenPetExists_PerformsSoftDeleteAndPublishes()
        {
            var pet = new Pet { Name = "Gale" };
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            var command = new DeletePetCommand(pet.Id);

            var result = await _handler.Handle(command, default);

            result.Should().BeTrue();

            var petInDb = await _context.Pets.IgnoreQueryFilters().FirstAsync(p => p.Id == pet.Id);
            petInDb.IsDeleted.Should().BeTrue();

            _mockPublisher.Verify(p => p.Publish(It.IsAny<PetChangedNotification>(), default), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenPetDoesNotExist_ReturnsFalse()
        {
            var command = new DeletePetCommand(999);

            var result = await _handler.Handle(command, default);

            result.Should().BeFalse();
            _mockPublisher.Verify(p => p.Publish(It.IsAny<PetChangedNotification>(), default), Times.Never);
        }

        public void Dispose() => _context.Dispose();
    }
}
