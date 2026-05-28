using DidWeFeedTheCatToday.Shared.Interfaces;

namespace DidWeFeedTheCatToday.Entities
{
    public class Pet : IAuditable
    {
        public int Id { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
        public required string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? Age { get; set; }
        public string? AdditionalInformation { get; set; }
        public List<Feeding> FeedingTimes { get; set; } = new List<Feeding>();
        public bool IsDeleted {  get; private set; }
        public DateTime? LastModifiedAt { get; set; }

        public void MarkAsDeleted() => IsDeleted = true;
        public void Restore() => IsDeleted = false;
    }
}
