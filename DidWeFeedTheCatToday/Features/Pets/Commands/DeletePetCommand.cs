using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Features.Pets.Notifications;
using MediatR;

namespace DidWeFeedTheCatToday.Features.Pets.Commands
{
    public record DeletePetCommand(int Id) : IRequest<bool>;

    public class DeletePetHandler(AppDbContext context, IPublisher publisher)
        : IRequestHandler<DeletePetCommand, bool>
    {
        /// <summary>
        /// Performs a soft-delete on a pet record.
        /// Marks the entity as deleted without physical removal to maintain audit integrity.
        /// </summary>
        public async Task<bool> Handle(DeletePetCommand request, CancellationToken ct)
        {
            var pet = await context.Pets.FindAsync([request.Id], ct);

            if (pet == null) 
                return false;

            pet.MarkAsDeleted();

            await context.SaveChangesAsync(ct);
            await publisher.Publish(new PetChangedNotification(), ct);

            return true;
        }
    }
}
