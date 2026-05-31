using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Features.Pets.Notifications;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using MediatR;

namespace DidWeFeedTheCatToday.Features.Pets
{
    public class AddPetHandler(AppDbContext context, IPublisher publisher) : IRequestHandler<AddPetCommand, GetPetDTO>
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

            await publisher.Publish(new PetChangedNotification(), cancellationToken);

            return new GetPetDTO { Id = pet.Id, Name = pet.Name };
        }
    }
}
