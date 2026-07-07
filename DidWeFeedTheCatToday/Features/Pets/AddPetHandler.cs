using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Features.Pets.Notifications;
using DidWeFeedTheCatToday.Mappers;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using MediatR;

namespace DidWeFeedTheCatToday.Features.Pets
{
    public class AddPetHandler(AppDbContext context, IPublisher publisher, PetMapper mapper) : IRequestHandler<AddPetCommand, GetPetDTO>
    {
        /// <summary>
        /// Handles the creation of a new pet entity.
        /// Persists data to the database and triggers a cache invalidation event.
        /// </summary>
        public async Task<GetPetDTO> Handle(AddPetCommand request, CancellationToken cancellationToken)
        {
            var pet = mapper.CommandToPet(request.PetDto);

            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            await publisher.Publish(new PetChangedNotification(), cancellationToken);

            return new GetPetDTO { Id = pet.Id, Name = pet.Name };
        }
    }
}
