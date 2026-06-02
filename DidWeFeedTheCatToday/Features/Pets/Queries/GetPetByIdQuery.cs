using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using MediatR;

namespace DidWeFeedTheCatToday.Features.Pets.Queries
{
    public record GetPetByIdQuery(int id) : IRequest<GetPetDTO?>;
}
