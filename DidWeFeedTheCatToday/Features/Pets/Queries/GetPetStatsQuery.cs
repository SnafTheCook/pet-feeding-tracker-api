using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Features.Pets.Queries
{
    public record GetPetStatsQuery : IRequest<PetStatsDTO>;

    public class GetPetStatsHandler(AppDbContext context) : IRequestHandler<GetPetStatsQuery, PetStatsDTO>
    {
        public async Task<PetStatsDTO> Handle(GetPetStatsQuery request, CancellationToken cancellationToken)
        {
            var total = await context.Pets.CountAsync(cancellationToken);

            var hungry = await context.Pets.CountAsync(p =>
                !p.FeedingTimes.Any(f => f.FeedingTime > DateTime.UtcNow.AddHours(-5)), cancellationToken);

            var fedToday = await context.Feedings.CountAsync(f =>
                f.FeedingTime > DateTime.UtcNow.Date, cancellationToken);

            return new PetStatsDTO(total, hungry, fedToday);
        }
    }

}
