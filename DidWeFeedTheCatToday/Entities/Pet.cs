namespace DidWeFeedTheCatToday.Entities
{
    public class Pet
    {
        public int Id { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public required string Name { get; set; }
        public DateTime? CreationDate { get; set; }
        public int? Age { get; set; }
        public string? AdditionalInformation { get; set; }
        public List<Feeding> FeedingTimes { get; set; } = new List<Feeding>();
    }
}
