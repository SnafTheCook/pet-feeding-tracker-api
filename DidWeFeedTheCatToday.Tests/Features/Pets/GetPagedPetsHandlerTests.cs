using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Data.Interceptors;
using DidWeFeedTheCatToday.Features.Pets.Queries;
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

        public void Dispose()
        {
            _context.Dispose();
            _cache.Dispose();
        }
    }
}
