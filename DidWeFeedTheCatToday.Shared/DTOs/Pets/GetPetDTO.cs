using DidWeFeedTheCatToday.Shared.Enums;

namespace DidWeFeedTheCatToday.Shared.DTOs.Pets
{
    public class GetPetDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime? CreationDate { get; set; }
        public int? Age { get; set; }
        public string? AdditionalInformation { get; set; }
        public DateTime? LastFed {  get; set; }
        public HungerStatus Status { get; set; }
    }
}
