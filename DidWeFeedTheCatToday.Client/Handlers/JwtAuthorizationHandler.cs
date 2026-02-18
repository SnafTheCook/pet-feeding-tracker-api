using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace DidWeFeedTheCatToday.Client.Handlers
{
    public class JwtAuthorizationHandler(IJSRuntime js) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await js.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
