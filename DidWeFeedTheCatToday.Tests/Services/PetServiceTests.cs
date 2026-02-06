using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.DTOs.Pets;
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

            var testPet = new Pet { Name = "Meowstarion" };
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

        [Fact]
        public async Task GetAllPetsAsync_WhenPetsExist_ReturnArray()
        {
            using var context = GetDbContext();
            var service = new PetService(context);

            var testPets = new List<Pet> 
            { 
                new Pet { Name = "Meowstarion"},
                new Pet { Name = "Katlach"},
                new Pet { Name = "Shadowcar"},
            };

            await context.AddRangeAsync(testPets);
            await context.SaveChangesAsync();

            var result = await service.GetAllPetsAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            result.Should().Contain(pet => pet.Name == "Meowstarion");
            result.Should().Contain(pet => pet.Name == "Katlach");
            result.Should().Contain(pet => pet.Name == "Shadowcar");
        }

        [Fact]
        public async Task GetAllPetsAsync_WhenNoPetsExist_ReturnsEmptyList()
        {
            using var context = GetDbContext();
            var service = new PetService(context);

            var result = await service.GetAllPetsAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task AddPetAsync_WhenValidDTOProvided_SavesToDatabaseAndReturnDTO()
        {
            using var context = GetDbContext();
            var service = new PetService(context);

            var commandDTO = new CommandPetDTO
            {
                Name = "Meowstarion",
                Age = 3,
                AdditionalInformation = "Sneaky one."
            };

            var result = await service.AddPetAsync(commandDTO);

            //checking returend GetPetDTO
            result.Should().NotBeNull();
            result.Name.Should().Be("Meowstarion");
            result.Id.Should().BeGreaterThan(0);

            result.CreationDate.Should().NotBeNull();
            result.CreationDate.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            //checking state in db
            var petInDb = await context.Pets.FirstOrDefaultAsync(pet => pet.Id == result.Id);
            petInDb.Should().NotBeNull();
            petInDb.Name.Should().Be("Meowstarion");
            petInDb.CreationDate.Should().NotBeNull();
        }

        [Fact]
        public async Task OverridePetAsync_WhenPetExists_OverrideData()
        {
            using var context = GetDbContext();
            var service = new PetService(context);

            var testPet = new Pet { Name = "Meowstarion" };
            context.Add(testPet);
            await context.SaveChangesAsync();

            var testCommandPetDto = new CommandPetDTO
            {
                Name = "Katlach"
            };

            var result = await service.OverridePetAsync(testPet.Id, testCommandPetDto);

            result.Success.Should().BeTrue();

            var overrodePet = await context.Pets.FirstOrDefaultAsync(pet => pet.Id == testPet.Id);

            overrodePet.Should().NotBeNull();
            overrodePet.Name.Should().Be("Katlach");
        }

        [Fact]
        public async Task OverridePetAsync_WhenPetDoesntExist_ReturnFalse()
        {
            using var context = GetDbContext();
            var service = new PetService(context);

            var testCommandPetDto = new CommandPetDTO
            {
                Name = "Katlach"
            };

            var result = await service.OverridePetAsync(9999, testCommandPetDto);

            result.Success.Should().BeFalse();
            result.Error.Should().Be(Common.ServiceResultError.NotFound);
        }

        [Fact]
        public async Task DeletePetAsync_WhenPetFoundAndDeleted_ReturnTrue()
        {
            using var context = GetDbContext();
            var service = new PetService(context);

            var testPet = new Pet { Name = "Meowstarion" };
            context.Add(testPet);
            await context.SaveChangesAsync();

            var result = await service.DeletePetAsync(testPet.Id);
            result.Should().BeTrue();

            var petInDb = await context.Pets.FindAsync(testPet.Id);
            petInDb.Should().BeNull();
        }

        [Fact]
        public async Task DeletePetAsync_WhenPetNotFound_ReturnFalse()
        {
            using var context = GetDbContext();
            var service = new PetService(context);

            var result = await service.DeletePetAsync(9999);

            result.Should().BeFalse();
        }

    }
}
