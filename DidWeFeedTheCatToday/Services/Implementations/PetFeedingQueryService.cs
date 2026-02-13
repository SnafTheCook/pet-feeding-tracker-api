using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Shared.DTOs.PetFeedings;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;

namespace DidWeFeedTheCatToday.Services.Implementations
{
    public class PetFeedingQueryService(AppDbContext context) : IPetFeedingQueryService
    {
        public async Task<IEnumerable<GetPetFeedingDTO>> GetAllPetFeedingsAsync()
        {
            return await context.Pets
                .AsNoTracking()
                .Include(f => f.FeedingTimes)
                .Select(x => PetToPetsWithFeedingsDTO(x))
                .ToListAsync();
        }

        public async Task<GetPetFeedingDTO?> GetPetFeedingsByIdAsync(int id)
        {
            var petWithFeedings = await context.Pets
                .AsNoTracking()
                .Include(f => f.FeedingTimes)
                .Select(x => PetToPetsWithFeedingsDTO(x))
                .FirstOrDefaultAsync(f => f.Id == id);

            if (petWithFeedings == null)
                return null;

            return petWithFeedings;
        }

        private static GetPetFeedingDTO PetToPetsWithFeedingsDTO(Pet petItem) =>
            new GetPetFeedingDTO
            {
                Id = petItem.Id,
                Name = petItem.Name,
                FeedingTimes = petItem.FeedingTimes
                    .Select(f => new GetFeedingDTO
                    {
                        Id = f.Id,
                        PetId = f.PetId,
                        FeedingTime = f.FeedingTime
                    })
                    .ToList()
            };
    }
}
