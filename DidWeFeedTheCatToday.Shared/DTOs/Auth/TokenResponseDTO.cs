namespace DidWeFeedTheCatToday.Shared.DTOs.Auth
{
    public class TokenResponseDTO
    {
        public required string AccessToken {  get; set; }
        public required string RefreshToken {  get; set; }
        public Guid RefreshTokenId { get; set; }
        public Guid UserId { get; set; }
    }
}
