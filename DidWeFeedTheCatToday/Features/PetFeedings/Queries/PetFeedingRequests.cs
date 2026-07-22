using DidWeFeedTheCatToday.Shared.DTOs.PetFeedings;
using MediatR;

namespace DidWeFeedTheCatToday.Features.PetFeedings.Queries
{
    public record GetPetFeedingsQuery : IRequest<IEnumerable<GetPetFeedingDTO>>;
    public record GetPetFeedingsByIdQuery(int Id) : IRequest<GetPetFeedingDTO?>;
}
