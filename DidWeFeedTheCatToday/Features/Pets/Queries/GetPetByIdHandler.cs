using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Mappers;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using DidWeFeedTheCatToday.Shared.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Features.Pets.Queries
{
    public class GetPetByIdHandler(AppDbContext context, PetMapper mapper) : IRequestHandler<GetPetByIdQuery, GetPetDTO?>
    {
        /// <summary>
        /// Fetches a single pet by its unique ID.
        /// Projects the result directly into a DTO for optimal performance.
        /// </summary>
        public async Task<GetPetDTO?> Handle(GetPetByIdQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var pet = await context.Pets
                .AsNoTracking()
                .Include(p => p.FeedingTimes)
                .FirstOrDefaultAsync(p => p.Id == request.id, cancellationToken);

            if (pet == null) 
                return null;

            return mapper.PetToGetPetDTO(pet);
        }
    }
}
