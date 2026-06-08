using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using DidWeFeedTheCatToday.Shared.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;


namespace DidWeFeedTheCatToday.Features.Pets.Queries
{
    public class GetPagedPetsHandler(AppDbContext context, IMemoryCache cache) : IRequestHandler<GetPagedPetsQuery, PagedResult<GetPetDTO>>
    {
        public async Task<PagedResult<GetPetDTO>> Handle(GetPagedPetsQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"pets_{request.Page}_{request.PageSize}_{request.SearchTerm}_{request.SortBy}";

            if (!cache.TryGetValue(cacheKey, out PagedResult<GetPetDTO>? cachedResult))
            {
                var now = DateTime.UtcNow;
                var query = context.Pets.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query = query.Where(p => p.Name.ToLower().Contains(request.SearchTerm.ToLower()));

                query = request.SortBy?.ToLower() switch
                {
                    "age" => query.OrderBy(p => p.Age),
                    "lastFed" => query.OrderByDescending(p => p.FeedingTimes.Max(f => f.FeedingTime)),
                    _ => query.OrderBy(p => p.Name)
                };

                var totalCount = await query.CountAsync(cancellationToken);

                var items = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Age,
                        p.AdditionalInformation,
                        p.RowVersion,
                        p.CreatedAt,
                        LastFed = p.FeedingTimes.OrderByDescending(f => f.FeedingTime).Select(f => f.FeedingTime).FirstOrDefault()
                    })
                    .ToListAsync(cancellationToken);

                var dtos = items.Select(i => new GetPetDTO
                {
                    Id = i.Id,
                    Name = i.Name,
                    Age = i.Age,
                    AdditionalInformation = i.AdditionalInformation,
                    RowVersion = i.RowVersion,
                    CreationDate = i.CreatedAt,
                    LastFed = i.LastFed,
                    Status = PetStatusCalculator.CalculateHunger(i.LastFed, now)
                });

                cachedResult = new PagedResult<GetPetDTO>
                {
                    Items = dtos,
                    TotalCount = totalCount,
                    CurrentPage = request.Page,
                    PageSize = request.PageSize
                };

                var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .AddExpirationToken(PetCacheHelper.GetExpirationToken());

                cache.Set(cacheKey, cachedResult, cacheOptions);
            }

            return cachedResult!;
        }
    }
}
