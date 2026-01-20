using DidWeFeedTheCatToday.Entities;

namespace DidWeFeedTheCatToday.DTOs.Auth
{
    public class RegisterResponseDTO
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public Roles Role { get; set; } = Roles.Child;
    }
}
