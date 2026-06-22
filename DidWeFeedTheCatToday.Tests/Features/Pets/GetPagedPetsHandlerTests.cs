using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Data.Interceptors;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Features.Pets;
using DidWeFeedTheCatToday.Features.Pets.Queries;
using DidWeFeedTheCatToday.Mappers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DidWeFeedTheCatToday.Tests.Features.Pets
{
    public class GetPagedPetsHandlerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly GetPagedPetsHandler _handler;
        private readonly UpdateAuditableInterceptor _auditingInterceptor = new();
        private readonly PetMapper _petMapper;
        public GetPagedPetsHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .AddInterceptors(_auditingInterceptor)
                .Options;

            _context = new AppDbContext(options);

            PetCacheHelper.InvalidateCache();

            _cache = new MemoryCache(new MemoryCacheOptions());
            _petMapper = new PetMapper();
            _handler = new GetPagedPetsHandler(_context, _cache, _petMapper);
        }

        [Fact]
        public async Task Handle_WhenRequestingSecondPage_ReturnsCorrectSlice()
        {
            for (int i = 1; i <= 7; i++)
            {
                _context.Pets.Add(new Pet { Name = $"Pet {i}" });
            }

            await _context.SaveChangesAsync();

            var query = new GetPagedPetsQuery(Page: 2, PageSize: 5, SearchTerm: null, SortBy: "name");

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(7);
        }

        [Fact]
        public async Task Handle_WhenCalledTwice_ReturnsCachedData()
        {
            _context.Pets.Add(new Pet { Name = "CacheTest" });
            await _context.SaveChangesAsync();

            var query = new GetPagedPetsQuery(1, 10, null, "name");

            await _handler.Handle(query, CancellationToken.None);

            _context.Pets.RemoveRange(_context.Pets);
            await _context.SaveChangesAsync();

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Items.Should().NotBeEmpty();
        }

        public void Dispose()
        {
            _context.Dispose();
            _cache.Dispose();
        }
    }
}
