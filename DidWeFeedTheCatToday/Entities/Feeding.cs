namespace DidWeFeedTheCatToday.Entities
{
    public class Feeding
    {
        public int Id { get; set; }
        public DateTime? FeedingTime { get; set; }
        public int PetId { get; set; }
        //public Pet? Pet { get; set; }
    }
}
