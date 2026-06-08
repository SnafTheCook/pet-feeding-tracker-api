using Microsoft.Extensions.Primitives;

namespace DidWeFeedTheCatToday.Features.Pets
{
    public class PetCacheHelper
    {
        private static CancellationTokenSource _resetCacheToken = new();

        public static IChangeToken GetExpirationToken() => new CancellationChangeToken(_resetCacheToken.Token);

        public static void InvalidateCache()
        {
            if (_resetCacheToken.IsCancellationRequested) return;

            _resetCacheToken.Cancel();
            _resetCacheToken.Dispose();
            _resetCacheToken = new CancellationTokenSource();
        }
    }
}
