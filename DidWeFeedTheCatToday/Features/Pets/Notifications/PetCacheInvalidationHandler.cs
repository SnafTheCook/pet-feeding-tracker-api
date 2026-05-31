using DidWeFeedTheCatToday.Services.Implementations;
using MediatR;

namespace DidWeFeedTheCatToday.Features.Pets.Notifications
{
    public class PetCacheInvalidationHandler : INotificationHandler<PetChangedNotification>
    {
        public Task Handle(PetChangedNotification notification, CancellationToken cancellationToken)
        {
            PetService.ClearPetCache();

            return Task.CompletedTask;
        }
    }
}
