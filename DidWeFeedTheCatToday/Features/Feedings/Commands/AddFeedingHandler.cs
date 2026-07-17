using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Hubs;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Notification.Domain.Events;

namespace DidWeFeedTheCatToday.Features.Feedings.Commands
{
    public record AddFeedingCommand(PostFeedingDTO Dto) : IRequest<GetFeedingDTO?>;

    public class AddFeedingHandler(
        AppDbContext context,
        IHubContext<PetHub> hubContext,
        IPublishEndpoint publishEndpoint) : IRequestHandler<AddFeedingCommand, GetFeedingDTO?>
    {
        public async Task<GetFeedingDTO?> Handle(AddFeedingCommand request, CancellationToken ct)
        {
            var pet = await context.Pets.FirstOrDefaultAsync(p => p.Id == request.Dto.PetId, ct);
            if (pet == null) return null;

            var feeding = new Feeding
            {
                PetId = request.Dto.PetId,
                FeedingTime = request.Dto.FeedingTime ?? DateTime.UtcNow
            };

            context.Feedings.Add(feeding);
            await context.SaveChangesAsync(ct);

            await hubContext.Clients.All.SendAsync("PetFed", feeding.PetId, feeding.FeedingTime, ct);

            await publishEndpoint.Publish(new PetFedEvent(
                Guid.NewGuid(),
                pet.Name,
                "owner@example.com",
                feeding.FeedingTime.Value
            ), ct);

            return new GetFeedingDTO
            {
                Id = feeding.Id,
                PetId = feeding.PetId,
                FeedingTime = feeding.FeedingTime
            };
        }
    }
}
