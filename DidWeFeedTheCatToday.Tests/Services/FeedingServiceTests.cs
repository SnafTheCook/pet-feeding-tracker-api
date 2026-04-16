using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Hubs;
using DidWeFeedTheCatToday.Services.Implementations;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace DidWeFeedTheCatToday.Tests.Services
{
    public class FeedingServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly FeedingService _service;
        private readonly Mock<IHubContext<PetHub>> _mockHubContext = new();
        private readonly Mock<IHubClients> _mockClients = new();
        private readonly Mock<IClientProxy> _mockClientProxy = new();
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint = new();

        public FeedingServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            _mockHubContext.Setup(hub => hub.Clients).Returns(_mockClients.Object);
            _mockClients.Setup(clients => clients.All).Returns(_mockClientProxy.Object);

            _service = new FeedingService(_context, _mockHubContext.Object, _mockPublishEndpoint.Object);
        }

        [Fact]
        public async Task GetFeedingsAsync_WhenFeedingsExist_ReturnsAllFeedingsAsDtos()
        {
            var testPet = new Pet { Name = "Meowstarion" };
            var testFeeding1 = new Feeding { PetId = testPet.Id, FeedingTime = DateTime.UtcNow.AddMinutes(-5) };
            var testFeeding2 = new Feeding { PetId = testPet.Id, FeedingTime = DateTime.UtcNow };

            _context.Pets.Add(testPet);
            _context.Feedings.AddRange(testFeeding1, testFeeding2);
            await _context.SaveChangesAsync();

            var result = await _service.GetFeedingsAsync();

            result.Should().NotBeNull();
            result.GetType().Should().BeAssignableTo<List<GetFeedingDTO>>();
            result.Count().Should().Be(2);
        }

        [Fact]
        public async Task GetFeedingByIdAsync_WhenFeedingExists_ReturnsCorrectDto()
        {
            var testPet = new Pet { Name = "Meowstarion" };
            _context.Pets.Add(testPet);

            var testFeeding = new Feeding { PetId = testPet.Id, FeedingTime = DateTime.UtcNow };
            _context.Feedings.Add(testFeeding);

            await _context.SaveChangesAsync();

            var result = await _service.GetFeedingByIdAsync(testFeeding.Id);

            result.Should().NotBeNull();
            result.GetType().Should().BeAssignableTo<GetFeedingDTO>();
            result.PetId.Should().Be(testPet.Id);
        }

        [Fact]
        public async Task GetFeedingByIdAsync_WhenFeedingDoesNotExist_ReturnsNull()
        {
            var result = await _service.GetFeedingByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddFeedingAsync_WhenPetExists_SavesFeedingAndReturnsDto()
        {
            var testPet = new Pet { Name = "Meowstarion" };

            _context.Pets.Add(testPet);
            await _context.SaveChangesAsync();

            var testFeedingDto = new PostFeedingDTO
            {
                PetId = testPet.Id,
                FeedingTime = DateTime.UtcNow
            };

            var result = await _service.AddFeedingAsync(testFeedingDto);

            result.Should().NotBeNull();
            result.PetId.Should().Be(testPet.Id);

            _context.Feedings.Count().Should().Be(1);

            _mockClientProxy.Verify(mock => mock.SendCoreAsync(
                "PetFed", It.Is<object[]>(obj => obj.Length == 2 && (int)obj[0] == testPet.Id),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddFeedingAsync_WhenPetDoesNotExist_ReturnsNull()
        {
            var testFeedingDto = new PostFeedingDTO { PetId = 9999 };

            var result = await _service.AddFeedingAsync(testFeedingDto);

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteFeedingAsync_WhenFeedingExists_RemovesFromDatabaseAndReturnsTrue()
        {
            var testPet = new Pet { Name = "Meowstarion" };
            _context.Pets.Add(testPet);

            var testFeeding = new Feeding 
            {
                PetId = testPet.Id, 
                FeedingTime = DateTime.UtcNow 
            };
            _context.Feedings.Add(testFeeding);

            await _context.SaveChangesAsync();

            var feedingInDb = await _context.Feedings.AnyAsync(feed => feed.Id == testFeeding.Id);
            feedingInDb.Should().BeTrue();

            var result = await _service.DeleteFeedingAsync(testFeeding.Id);
            result.Should().BeTrue();

            var feedingInDbAfterDelete = await _context.Feedings.AnyAsync(feed => feed.Id == testFeeding.Id);
            feedingInDbAfterDelete.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteFeedingAsync_WhenFeedingDoesNotExist_ReturnsFalse()
        {
            var result = await _service.DeleteFeedingAsync(9999);
            result.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
