using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using MediatR;

namespace DidWeFeedTheCatToday.Features.Pets
{
    public record AddPetCommand(CommandPetDTO PetDto) : IRequest<GetPetDTO>;
}
