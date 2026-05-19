using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Implementations;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DidWeFeedTheCatToday.Tests.Services
{
    public class PetServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly PetService _service;
        private readonly IMemoryCache _cache;
        public PetServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            
            _cache = new MemoryCache(new MemoryCacheOptions());
            _service = new PetService(_context, _cache);
        }

        [Fact]
        public async Task GetPetByIdAsync_WhenPetExists_ReturnsPetDto()
        {
            var testPet = new Pet { Name = "Meowstarion" };
            _context.Add(testPet);
            await _context.SaveChangesAsync();

            var result = await _service.GetPetByIdAsync(1);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Meowstarion");
            result.Id.Should().Be(1);
        }
        
        [Fact]
        public async Task GetPetByIdAsync_WhenPetDoesNotExist_ReturnsNull()
        {
            var result = await _service.GetPetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllPetsAsync_WhenPetsExist_ReturnArray()
        {
            var testPets = new List<Pet> 
            { 
                new Pet { Name = "Meowstarion"},
                new Pet { Name = "Katlach"},
                new Pet { Name = "Shadowcar"},
            };

            await _context.AddRangeAsync(testPets);
            await _context.SaveChangesAsync();

            var result = await _service.GetAllPetsAsync();

            result.Should().NotBeNull();
            result.Should().HaveCount(3);

            result.Should().Contain(pet => pet.Name == "Meowstarion");
            result.Should().Contain(pet => pet.Name == "Katlach");
            result.Should().Contain(pet => pet.Name == "Shadowcar");
        }

        [Fact]
        public async Task GetAllPetsAsync_WhenNoPetsExist_ReturnsEmptyList()
        {
            var result = await _service.GetAllPetsAsync();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPagedPetsAsync_WhenCalledTwice_ReturnsCachedDataEvenIfDbIsCleared()
        {
            var pet = new Pet { Name = "cacheTest" };
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            await _service.GetPagedPetsAsync(1, 10, null, "name");

            _context.Pets.RemoveRange(_context.Pets);
            await _context.SaveChangesAsync();

            var result = await _service.GetPagedPetsAsync(1, 10, null, "name");

            result.Items.Should().NotBeEmpty();
            result.Items.First().Name.Should().Be("cacheTest");
        }

        [Fact]
        public async Task AddPetAsync_WhenValidDTOProvided_SavesToDatabaseAndReturnDTO()
        {
            var commandDTO = new CommandPetDTO
            {
                Name = "Meowstarion",
                Age = 3,
                AdditionalInformation = "Sneaky one."
            };

            var result = await _service.AddPetAsync(commandDTO);

            result.Should().NotBeNull();
            result.Name.Should().Be("Meowstarion");
            result.Id.Should().BeGreaterThan(0);

            result.CreationDate.Should().NotBeNull();
            result.CreationDate.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var petInDb = await _context.Pets.FirstOrDefaultAsync(pet => pet.Id == result.Id);
            petInDb.Should().NotBeNull();
            petInDb.Name.Should().Be("Meowstarion");
            petInDb.CreationDate.Should().NotBeNull();
        }

        [Fact]
        public async Task AddPetAsync_WhenNewPetAdded_ShouldInvalidateCache()
        {
            await _service.GetPagedPetsAsync(1, 10, null, "name");

            var newPet = new CommandPetDTO { Name = "newPet" };
            await _service.AddPetAsync(newPet);

            var result = await _service.GetPagedPetsAsync(1, 10, null, "name");

            result.Items.Should().Contain(p => p.Name == "newPet");
        }

        [Fact]
        public async Task OverridePetAsync_WhenPetExists_OverrideData()
        {
            var testPet = new Pet { Name = "Meowstarion" };
            _context.Add(testPet);
            await _context.SaveChangesAsync();

            var testCommandPetDto = new CommandPetDTO
            {
                Name = "Katlach"
            };

            var result = await _service.OverridePetAsync(testPet.Id, testCommandPetDto);

            result.Success.Should().BeTrue();

            var overrodePet = await _context.Pets.FirstOrDefaultAsync(pet => pet.Id == testPet.Id);

            overrodePet.Should().NotBeNull();
            overrodePet.Name.Should().Be("Katlach");
        }

        [Fact]
        public async Task OverridePetAsync_WhenPetDoesntExist_ReturnFalse()
        {
            var testCommandPetDto = new CommandPetDTO
            {
                Name = "Katlach"
            };

            var result = await _service.OverridePetAsync(9999, testCommandPetDto);

            result.Success.Should().BeFalse();
            result.Error.Should().Be(ServiceResultError.NotFound);
        }

        [Fact]
        public async Task DeletePetAsync_WhenPetFoundAndDeleted_ReturnTrue()
        {
            var testPet = new Pet { Name = "Meowstarion" };
            _context.Add(testPet);
            await _context.SaveChangesAsync();

            var result = await _service.DeletePetAsync(testPet.Id);
            result.Should().BeTrue();

            var petInDb = await _context.Pets.FindAsync(testPet.Id);
            petInDb.Should().BeNull();
        }

        [Fact]
        public async Task DeletePetAsync_WhenPetNotFound_ReturnFalse()
        {
            var result = await _service.DeletePetAsync(9999);

            result.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Dispose();
            _cache.Dispose();
        }
    }
}
