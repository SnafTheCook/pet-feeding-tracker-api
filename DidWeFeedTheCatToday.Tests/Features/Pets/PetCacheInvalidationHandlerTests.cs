using DidWeFeedTheCatToday.Features.Pets;
using DidWeFeedTheCatToday.Features.Pets.Notifications;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;

namespace DidWeFeedTheCatToday.Tests.Features.Pets
{
    public class PetCacheInvalidationHandlerTests : IDisposable
    {
        private readonly IMemoryCache _cache;
        private readonly PetCacheInvalidationHandler _handler;

        public PetCacheInvalidationHandlerTests()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _handler = new PetCacheInvalidationHandler();

            PetCacheHelper.InvalidateCache();
        }

        [Fact]
        public async Task Handle_WhenPetChanged_InvalidatesCache()
        {
            var cacheKey = "test_key";
            var options = new MemoryCacheEntryOptions()
                .AddExpirationToken(PetCacheHelper.GetExpirationToken());

            _cache.Set(cacheKey, "some_data", options);
            _cache.Get(cacheKey).Should().NotBeNull();

            await _handler.Handle(new PetChangedNotification(), CancellationToken.None);

            _cache.Get(cacheKey).Should().BeNull();
        }
        public void Dispose()
        {
            _cache.Dispose();
        }
    }
}
