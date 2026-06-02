using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using DidWeFeedTheCatToday.Shared.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Features.Pets.Queries
{
    public class GetPetByIdHandler(AppDbContext context) : IRequestHandler<GetPetByIdQuery, GetPetDTO?>
    {
        public async Task<GetPetDTO?> Handle(GetPetByIdQuery request, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            return await context.Pets
                .AsNoTracking()
                .Where(pet => pet.Id == request.id)
                .Select(pet => new GetPetDTO
                {
                    Id = pet.Id,
                    Name = pet.Name,
                    Age = pet.Age,
                    AdditionalInformation = pet.AdditionalInformation,
                    CreationDate = pet.CreatedAt,
                    RowVersion = pet.RowVersion,

                    LastFed = pet.FeedingTimes
                    .OrderByDescending(feeding => feeding.FeedingTime)
                    .Select(feeding => feeding.FeedingTime)
                    .FirstOrDefault(),

                    Status = PetStatusCalculator.CalculateHunger(pet.FeedingTimes
                    .OrderByDescending(feeding => feeding.FeedingTime)
                    .Select(feeding => feeding.FeedingTime)
                    .FirstOrDefault(), now)
                })
                .FirstOrDefaultAsync();
        }
    }
}
