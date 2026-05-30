using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using MediatR;

namespace DidWeFeedTheCatToday.Features.Pets
{
    public class AddPetHandler(AppDbContext context) : IRequestHandler<AddPetCommand, GetPetDTO>
    {
        public async Task<GetPetDTO> Handle(AddPetCommand request, CancellationToken cancellationToken)
        {
            var pet = new Pet
            {
                Name = request.PetDto.Name,
                Age = request.PetDto.Age
            };

            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            return new GetPetDTO { Id = pet.Id, Name = pet.Name };
        }
    }
}
