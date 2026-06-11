using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Data.Interceptors;
using DidWeFeedTheCatToday.Features.Pets;
using DidWeFeedTheCatToday.Features.Pets.Notifications;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
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
    public class AddPetHandlerTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IPublisher> _mockPublisher = new();
        private readonly AddPetHandler _handler;

        public AddPetHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddInterceptors(new UpdateAuditableInterceptor()).Options;

            _context = new AppDbContext(options);
            _handler = new AddPetHandler(_context, _mockPublisher.Object);
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
