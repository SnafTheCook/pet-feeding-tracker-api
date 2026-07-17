using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Features.Feedings.Queries
{
    public record GetFeedingsQuery : IRequest<IEnumerable<GetFeedingDTO>>;
    public record GetFeedingByIdQuery(int Id) : IRequest<GetFeedingDTO?>;

    public class GetFeedingHandlers(AppDbContext context)
        : IRequestHandler<GetFeedingsQuery, IEnumerable<GetFeedingDTO>>,
          IRequestHandler<GetFeedingByIdQuery, GetFeedingDTO?>
    {
        public async Task<IEnumerable<GetFeedingDTO>> Handle(GetFeedingsQuery request, CancellationToken ct)
        {
            return await context.Feedings
                .AsNoTracking()
                .Select(f => new GetFeedingDTO { Id = f.Id, PetId = f.PetId, FeedingTime = f.FeedingTime })
                .ToListAsync(ct);
        }

        public async Task<GetFeedingDTO?> Handle(GetFeedingByIdQuery request, CancellationToken ct)
        {
            return await context.Feedings
                .AsNoTracking()
                .Where(f => f.Id == request.Id)
                .Select(f => new GetFeedingDTO { Id = f.Id, PetId = f.PetId, FeedingTime = f.FeedingTime })
                .FirstOrDefaultAsync(ct);
        }
    }
}
