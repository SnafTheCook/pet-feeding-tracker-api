using DidWeFeedTheCatToday.Shared.DTOs.Feedings;

namespace DidWeFeedTheCatToday.Shared.DTOs.PetFeedings
{
    public class GetPetFeedingDTO
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public List<GetFeedingDTO> FeedingTimes { get; set; } = new List<GetFeedingDTO>();
    }
}
