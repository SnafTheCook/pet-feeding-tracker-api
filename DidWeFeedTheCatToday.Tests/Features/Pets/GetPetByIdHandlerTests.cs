using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Features.Pets.Queries;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Tests.Features.Pets
{
    public class GetPetByIdHandlerTests
    {
        private readonly AppDbContext _context;
        private readonly GetPetByIdHandler _handler;

        public GetPetByIdHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            _context = new AppDbContext(options);
            _handler = new GetPetByIdHandler(_context);
        }

        [Fact]
        public async Task Handle_WhenFound_ReturnsDto()
        {
            var pet = new Pet { Name = "Gale" };
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            var result = await _handler.Handle(new GetPetByIdQuery(pet.Id), default);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Gale");
        }

        [Fact]
        public async Task Handle_WhenMissing_ReturnsNull()
        {
            var result = await _handler.Handle(new GetPetByIdQuery(999), default);
            result.Should().BeNull();
        }
    }
}
