namespace DidWeFeedTheCatToday.Shared.DTOs.Auth
{
    public class RefreshTokenRequestDTO
    {
        public Guid UserId { get; set; }
        public Guid RefreshTokenId { get; set; }
        public required string RefreshToken { get; set; }
    }
}
