using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Data.Interceptors;
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
        private readonly UpdateAuditableInterceptor _auditingInterceptor = new();
        public PetServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .AddInterceptors(_auditingInterceptor)
                .Options;

            _context = new AppDbContext(options);
            
            _cache = new MemoryCache(new MemoryCacheOptions());
            _service = new PetService(_context, _cache);
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

            var petInDb = await _context.Pets
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == testPet.Id);

            petInDb.Should().NotBeNull();
            petInDb!.IsDeleted.Should().BeTrue();

            var petHidden = await _context.Pets.FirstOrDefaultAsync(p => p.Id == testPet.Id);
            petHidden.Should().BeNull();
        }

        [Fact]
        public async Task DeletePetAsync_WhenPetNotFound_ReturnFalse()
        {
            var result = await _service.DeletePetAsync(9999);

            result.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _cache.Dispose();
        }
    }
}
