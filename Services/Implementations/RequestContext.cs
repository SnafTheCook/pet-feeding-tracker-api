using DidWeFeedTheCatToday.Services.Interfaces;

namespace DidWeFeedTheCatToday.Services.Implementations
{
    public class RequestContext(IHttpContextAccessor accessor) : IRequestContext
    {
        public string? IpAddress => accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }
}
