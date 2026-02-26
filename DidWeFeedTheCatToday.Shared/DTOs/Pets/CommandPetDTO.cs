using System.ComponentModel.DataAnnotations;

namespace DidWeFeedTheCatToday.Shared.DTOs.Pets
{
    public class CommandPetDTO
    {
        [Required(ErrorMessage = "Pet's name is required.")]
        [StringLength(30, ErrorMessage = "Pet's name is too long.")]
        public required string Name { get; set; }
        [Range(0, 200, ErrorMessage = "Age must be between 0 and 200.")]
        public int? Age { get; set; }
        [StringLength(500, ErrorMessage = "Additional information is too long.")]
        public string? AdditionalInformation { get; set; }
    }
}
