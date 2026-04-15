using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using Microsoft.AspNetCore.SignalR;
using DidWeFeedTheCatToday.Hubs;
using MassTransit;
using Notification.Domain.Events;

namespace DidWeFeedTheCatToday.Services.Implementations
{
    public class FeedingService(
        AppDbContext context, 
        IHubContext<PetHub> hubContext,
        IPublishEndpoint publishEndpoint) : IFeedingService
    {
        /// <summary>
        /// Retrieves a collection of feedings. Query is untracked.
        /// </summary>
        /// <returns>a <see cref="List{T}"/> of <see cref="GetFeedingDTO"/></returns>
        public async Task<IEnumerable<GetFeedingDTO>> GetFeedingsAsync()
        {
            return await context.Feedings
                .AsNoTracking() //query is read onyl so it doesn't need tracking. better performance
                .Select(f => FeedingToGetFeedingDTO(f))
                .ToListAsync();
        }
        /// <summary>
        /// Retrieves a single feeding.
        /// </summary>
        /// <param name="id">Unique identifier of a feeding.</param>
        /// <returns>a <see cref="GetFeedingDTO"/> on success. Returns <see langword="null"/> on failure.</returns>
        public async Task<GetFeedingDTO?> GetFeedingByIdAsync(int id)
        {
            var feeding = await context.Feedings.FindAsync(id);

            if (feeding == null)
                return null;

            return FeedingToGetFeedingDTO(feeding);
        }
        /// <summary>
        /// Persists a new feeding if a target pet exists.
        /// </summary>
        /// <param name="feeding">Feeding data</param>
        /// <returns>a <see cref="GetFeedingDTO"/> on success. Returns <see langword="null"/> if pet not found.</returns>
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

            var pet = await context.Pets.FindAsync(feeding.PetId);
            await publishEndpoint.Publish(new PetFedEvent(
                Guid.NewGuid(),
                pet?.Name ?? "Unknown",
                "owner@example.com",
                DateTime.UtcNow
                ));

            return FeedingToGetFeedingDTO(request);
        }
        /// <summary>
        /// Removes a feeding from history.
        /// </summary>
        /// <param name="id">Unique identifier of the feeding.</param>
        /// <returns>a <see langword="bool"/> indicating success or failure.</returns>
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
