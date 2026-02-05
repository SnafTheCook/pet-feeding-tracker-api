namespace DidWeFeedTheCatToday.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public Roles Role { get; set; } = Roles.Child;

        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiryDate { get; set; }
        public Guid? RefreshTokenId { get; set; }

        public DateTime? RefreshTokenCreatedAt { get; set; }
        public string? RefreshTokenCreatedByIp { get; set; }
    }

    public enum Roles
    {
        Admin,
        Parent,
        Child
    }
}
