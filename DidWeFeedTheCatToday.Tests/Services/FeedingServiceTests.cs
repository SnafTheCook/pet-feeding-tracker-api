using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Implementations;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Tests.Services
{
    public class FeedingServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetFeedingsAsync_WhenFeedingsExist_ReturnsAllFeedingsAsDtos()
        {
            using var context = GetDbContext();
            var service = new FeedingService(context);

            var testPet = new Pet { Name = "Meowstarion" };
            var testFeeding1 = new Feeding { PetId = testPet.Id, FeedingTime = DateTime.UtcNow.AddMinutes(-5) };
            var testFeeding2 = new Feeding { PetId = testPet.Id, FeedingTime = DateTime.UtcNow };

            context.Pets.Add(testPet);
            context.Feedings.AddRange(testFeeding1, testFeeding2);
            await context.SaveChangesAsync();

            var result = await service.GetFeedingsAsync();

            result.Should().NotBeNull();
            result.GetType().Should().BeAssignableTo<List<GetFeedingDTO>>();
            result.Count().Should().Be(2);
        }

        [Fact]
        public async Task GetFeedingByIdAsync_WhenFeedingExists_ReturnsCorrectDto()
        {
            using var context = GetDbContext();
            var service = new FeedingService(context);

            var testPet = new Pet { Name = "Meowstarion" };
            context.Pets.Add(testPet);

            var testFeeding = new Feeding { PetId = testPet.Id, FeedingTime = DateTime.UtcNow };
            context.Feedings.Add(testFeeding);

            await context.SaveChangesAsync();

            var result = await service.GetFeedingByIdAsync(testFeeding.Id);

            result.Should().NotBeNull();
            result.GetType().Should().BeAssignableTo<GetFeedingDTO>();
            result.PetId.Should().Be(testPet.Id);
        }

        [Fact]
        public async Task GetFeedingByIdAsync_WhenFeedingDoesNotExist_ReturnsNull()
        {
            using var context = GetDbContext();
            var service = new FeedingService(context);

            var result = await service.GetFeedingByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AddFeedingAsync_WhenPetExists_SavesFeedingAndReturnsDto()
        {
            using var context = GetDbContext();
            var service = new FeedingService(context);

            var testPet = new Pet { Name = "Meowstarion" };

            context.Pets.Add(testPet);
            await context.SaveChangesAsync();

            var testFeedingDto = new PostFeedingDTO
            {
                PetId = testPet.Id,
                FeedingTime = DateTime.UtcNow
            };

            var result = await service.AddFeedingAsync(testFeedingDto);

            result.Should().NotBeNull();
            result.PetId.Should().Be(testPet.Id);

            context.Feedings.Count().Should().Be(1);
        }

        [Fact]
        public async Task AddFeedingAsync_WhenPetDoesNotExist_ReturnsNull()
        {
            using var context = GetDbContext();
            var service = new FeedingService(context);

            var testFeedingDto = new PostFeedingDTO { PetId = 9999 };

            var result = await service.AddFeedingAsync(testFeedingDto);

            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteFeedingAsync_WhenFeedingExists_RemovesFromDatabaseAndReturnsTrue()
        {
            using var context = GetDbContext();
            var service = new FeedingService(context);

            var testPet = new Pet { Name = "Meowstarion" };
            context.Pets.Add(testPet);

            var testFeeding = new Feeding 
            {
                PetId = testPet.Id, 
                FeedingTime = DateTime.UtcNow 
            };
            context.Feedings.Add(testFeeding);

            await context.SaveChangesAsync();

            var feedingInDb = await context.Feedings.AnyAsync(feed => feed.Id == testFeeding.Id);
            feedingInDb.Should().BeTrue();

            var result = await service.DeleteFeedingAsync(testFeeding.Id);
            result.Should().BeTrue();

            var feedingInDbAfterDelete = await context.Feedings.AnyAsync(feed => feed.Id == testFeeding.Id);
            feedingInDbAfterDelete.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteFeedingAsync_WhenFeedingDoesNotExist_ReturnsFalse()
        {
            using var context = GetDbContext();
            var service = new FeedingService(context);

            var result = await service.DeleteFeedingAsync(9999);
            result.Should().BeFalse();
        }
    }
}
