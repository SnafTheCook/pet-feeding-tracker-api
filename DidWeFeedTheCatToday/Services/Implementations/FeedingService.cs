using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using Microsoft.AspNetCore.SignalR;
using DidWeFeedTheCatToday.Hubs;

namespace DidWeFeedTheCatToday.Services.Implementations
{
    public class FeedingService(AppDbContext context, IHubContext<PetHub> hubContext) : IFeedingService
    {
        public async Task<IEnumerable<GetFeedingDTO>> GetFeedingsAsync()
        {
            return await context.Feedings
                .AsNoTracking() //query is read onyl so it doesn't need tracking. better performance
                .Select(f => FeedingToGetFeedingDTO(f))
                .ToListAsync();
        }

        public async Task<GetFeedingDTO?> GetFeedingByIdAsync(int id)
        {
            var feeding = await context.Feedings.FindAsync(id);

            if (feeding == null)
                return null;

            return FeedingToGetFeedingDTO(feeding);
        }

        public async Task<GetFeedingDTO?> AddFeedingAsync(PostFeedingDTO feeding)
        {
            if (!await context.Pets.AnyAsync(p => p.Id == feeding.PetId))
                return null;

            var request = new Feeding
            {
                PetId = feeding.PetId,
                FeedingTime = feeding.FeedingTime ?? DateTime.UtcNow
            };

            context.Feedings.Add(request);
            await context.SaveChangesAsync();

            await hubContext.Clients.All.SendAsync("PetFed", request.PetId, request.FeedingTime);

            return FeedingToGetFeedingDTO(request);
        }

        public async Task<bool> DeleteFeedingAsync(int id)
        {
            var feeding = await context.Feedings.FindAsync(id);
            if (feeding == null)
                return false;

            context.Feedings.Remove(feeding);
            await context.SaveChangesAsync();

            return true;
        }

        private static GetFeedingDTO FeedingToGetFeedingDTO(Feeding feeding) =>
            new GetFeedingDTO
            {
                Id = feeding.Id,
                PetId = feeding.PetId,
                FeedingTime = feeding.FeedingTime
            };
    }
}
