namespace DidWeFeedTheCatToday.DTOs.Pets
{
    public class GetPetDTO
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime? CreationDate { get; set; }
        public int? Age { get; set; }
        public string? AdditionalInformation { get; set; }
    }
}
