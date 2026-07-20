using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using DidWeFeedTheCatToday.Shared.DTOs.PetFeedings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Features.PetFeedings.Queries
{
    public class GetPetFeedingsByIdHandler(AppDbContext context)
    : IRequestHandler<GetPetFeedingsByIdQuery, GetPetFeedingDTO?>
    {
        public async Task<GetPetFeedingDTO?> Handle(GetPetFeedingsByIdQuery request, CancellationToken ct)
        {
            return await context.Pets
                .AsNoTracking()
                .Include(p => p.FeedingTimes)
                .Where(p => p.Id == request.Id)
                .Select(p => new GetPetFeedingDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    FeedingTimes = p.FeedingTimes.Select(f => new GetFeedingDTO { ... }).ToList()
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
