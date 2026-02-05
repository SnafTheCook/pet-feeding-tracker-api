using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidWeFeedTheCatToday.Tests.Services
{
    public class PetServiceTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task GetPetByIdAsync_WhenPetExists_ReturnsPetDto()
        {
            using var context = GetDbContext();
            var service = new PetService(context);

            var testPet = new Pet { Id = 1, Name = "Meowstarion" };
            context.Add(testPet);
            await context.SaveChangesAsync();

            var result = await service.GetPetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Meowstarion");
            result.Id.Should().Be(1);
        }
        
        [Fact]
        public async Task GetPetByIdAsync_WhenPetDoesNotExist_ReturnsNull()
        {
            using var context = GetDbContext();
            var service = new PetService(context);

            var result = await service.GetPetByIdAsync(9999);

            result.Should().BeNull();
        }
    }
}
