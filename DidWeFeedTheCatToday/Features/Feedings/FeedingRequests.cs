namespace DidWeFeedTheCatToday.Features.Feedings
{
    using MediatR;
    using DidWeFeedTheCatToday.Shared.DTOs.Feedings;

    public record GetFeedingsQuery : IRequest<IEnumerable<GetFeedingDTO>>;
    public record GetFeedingByIdQuery(int Id) : IRequest<GetFeedingDTO?>;
    public record AddFeedingCommand(PostFeedingDTO DTO) : IRequest<GetFeedingDTO?>;
    public record DeleteFeedingCommand(int Id) : IRequest<bool>;
}
