using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using DidWeFeedTheCatToday.Shared.DTOs.PetFeedings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Features.PetFeedings.Queries
{
    public class GetPetFeedingsHandler(AppDbContext context)
    : IRequestHandler<GetPetFeedingsQuery, IEnumerable<GetPetFeedingDTO>>
    {
        public async Task<IEnumerable<GetPetFeedingDTO>> Handle(GetPetFeedingsQuery request, CancellationToken ct)
        {
            return await context.Pets
                .AsNoTracking()
                .Include(p => p.FeedingTimes)
                .Select(p => new GetPetFeedingDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    FeedingTimes = p.FeedingTimes.Select(f => new GetFeedingDTO
                    {
                        Id = f.Id,
                        PetId = f.PetId,
                        FeedingTime = f.FeedingTime
                    }).ToList()
                })
                .ToListAsync(ct);
        }
    }
}
