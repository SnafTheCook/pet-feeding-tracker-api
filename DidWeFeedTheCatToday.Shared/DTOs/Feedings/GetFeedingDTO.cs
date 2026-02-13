namespace DidWeFeedTheCatToday.Shared.DTOs.Feedings
{
    public class GetFeedingDTO
    {
        public int Id { get; set; }
        public DateTime? FeedingTime { get; set; }
        public int PetId { get; set; }
    }
}
