using DidWeFeedTheCatToday.DTOs.Feedings;

namespace DidWeFeedTheCatToday.Services.Interfaces
{
    public interface IFeedingService
    {
        Task<IEnumerable<GetFeedingDTO>> GetFeedingsAsync();
        Task<GetFeedingDTO?> GetFeedingByIdAsync(int id);
        Task<GetFeedingDTO?> AddFeedingAsync(PostFeedingDTO feeding);
        Task<bool> DeleteFeedingAsync(int id);
    }
}
