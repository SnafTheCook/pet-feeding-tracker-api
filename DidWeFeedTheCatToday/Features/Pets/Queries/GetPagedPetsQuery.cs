using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using MediatR;

namespace DidWeFeedTheCatToday.Features.Pets.Queries
{
    public record GetPagedPetsQuery(
        int Page,
        int PageSize,
        string? SearchTerm,
        string? SortBy) : IRequest<PagedResult<GetPetDTO>>;
}
