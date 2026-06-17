using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Features.Pets.Notifications;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Features.Pets.Commands
{
    public record UpdatePetCommand(int Id, CommandPetDTO PetDto) : IRequest<ServiceResult>;

    public class UpdatePetHandler(AppDbContext context, IPublisher publisher)
        : IRequestHandler<UpdatePetCommand, ServiceResult>
    {
        public async Task<ServiceResult> Handle(UpdatePetCommand request, CancellationToken ct)
        {
            var pet = await context.Pets.FindAsync([request.Id], ct);

            if (pet == null) 
                return ServiceResult.Fail(ServiceResultError.NotFound);

            pet.Name = request.PetDto.Name;
            pet.Age = request.PetDto.Age;
            pet.AdditionalInformation = request.PetDto.AdditionalInformation;

            if (request.PetDto.RowVersion != null)
            {
                context.Entry(pet).Property(p => p.RowVersion).OriginalValue = request.PetDto.RowVersion;
            }

            try
            {
                await context.SaveChangesAsync(ct);
                await publisher.Publish(new PetChangedNotification(), ct);
                return ServiceResult.Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                return ServiceResult.Fail(ServiceResultError.ConcurrencyConflict);
            }
        }
    }
}
