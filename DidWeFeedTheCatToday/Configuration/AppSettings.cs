namespace DidWeFeedTheCatToday.Configuration
{
    public class AppSettings
    {
        public string Token { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience {  get; set; } = string.Empty;
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    }
}
