using DidWeFeedTheCatToday.Client.Services;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace DidWeFeedTheCatToday.Client.Handlers
{
    public class JwtAuthorizationHandler(IJSRuntime js, IServiceProvider serviceProvider) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool isAuthRequest = request.RequestUri?.AbsolutePath.Contains("api/auth") ?? false;

            if (!isAuthRequest)
            {
                var token = await js.InvokeAsync<string>("localStorage.getItem", "authToken");

                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (!isAuthRequest && response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var authService = serviceProvider.GetRequiredService<AuthService>();

                var refreshed = await authService.RefreshToken();

                if (refreshed)
                {
                    var newToken = await js.InvokeAsync<string>("localStorage.getItem", "authToken");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                    response.Dispose();
                    return await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }
    }
}
