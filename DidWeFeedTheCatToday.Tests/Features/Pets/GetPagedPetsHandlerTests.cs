using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Data.Interceptors;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Features.Pets.Queries;
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
        public GetPagedPetsHandlerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .AddInterceptors(_auditingInterceptor)
                .Options;

            _context = new AppDbContext(options);

            _cache = new MemoryCache(new MemoryCacheOptions());
            _handler = new GetPagedPetsHandler(_context, _cache);
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

        public void Dispose()
        {
            _context.Dispose();
            _cache.Dispose();
        }
    }
}
