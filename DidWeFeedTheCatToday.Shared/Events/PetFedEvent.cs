namespace Notification.Domain.Events
{
    public record PetFedEvent(
        Guid petId,
        string PetName,
        string OwnerEmail,
        DateTime FedTime
    );
}
