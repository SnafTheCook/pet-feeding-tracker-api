using DidWeFeedTheCatToday.Shared.DTOs.PetFeedings;

namespace DidWeFeedTheCatToday.Services.Interfaces
{
    public interface IPetFeedingQueryService
    {
        Task<IEnumerable<GetPetFeedingDTO>> GetAllPetFeedingsAsync();
        Task<GetPetFeedingDTO?> GetPetFeedingsByIdAsync(int id);
    }
}
