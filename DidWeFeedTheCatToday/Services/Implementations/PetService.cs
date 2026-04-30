using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using DidWeFeedTheCatToday.Shared.Enums;
using MassTransit.Futures.Contracts;
using Microsoft.EntityFrameworkCore;

namespace DidWeFeedTheCatToday.Services.Implementations
{
    public class PetService(AppDbContext context) : IPetService
    {
        /// <summary>
        /// Retrieves a list of all pets.
        /// </summary>
        /// <returns>A collection of <see cref="GetPetDTO"/>. Returns an empty list if no pets exist.</returns>
        public async Task<IEnumerable<GetPetDTO>> GetAllPetsAsync()
        {
            var now = DateTime.UtcNow;

            return await context.Pets
                .AsNoTracking()
                .Select(pet => new GetPetDTO
                {
                    Id = pet.Id,
                    Name = pet.Name,
                    Age = pet.Age,
                    AdditionalInformation = pet.AdditionalInformation,
                    CreationDate = pet.CreationDate,
                    RowVersion = pet.RowVersion,

                    LastFed = pet.FeedingTimes
                    .OrderByDescending(feeding => feeding.FeedingTime)
                    .Select(feeding => feeding.FeedingTime)
                    .FirstOrDefault(),

                    Status = CalculateHunger(pet.FeedingTimes
                    .OrderByDescending(feeding => feeding.FeedingTime)
                    .Select(feeding => feeding.FeedingTime)
                    .FirstOrDefault(), now)
                })
                .ToListAsync();
        }

        public async Task<PagedResult<GetPetDTO>> GetPagedPetsAsync(int page, int pageSize, string? searchTerm, string? sortBy)
        {
            var query = context.Pets.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>  p.Name.ToLower().Contains(searchTerm.ToLower()));
            }

            var totalCount = await query.CountAsync();

            query = sortBy switch
            {
                "age" => query.OrderBy(p => p.Age),
                "lastFed" => query.OrderByDescending(p => p.FeedingTimes.OrderByDescending(f => f.FeedingTime).Select(f => f.FeedingTime)),
                _ => query.OrderBy(p => p.Name)
            };

            var items = await query
                .OrderBy(pet => pet.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(pet => new GetPetDTO {
                    Id = pet.Id,
                    Name = pet.Name,
                    Age = pet.Age,
                    AdditionalInformation = pet.AdditionalInformation,
                    CreationDate = pet.CreationDate,
                    RowVersion = pet.RowVersion,

                    LastFed = pet.FeedingTimes
                    .OrderByDescending(feeding => feeding.FeedingTime)
                    .Select(feeding => feeding.FeedingTime)
                    .FirstOrDefault(),

                    Status = CalculateHunger(pet.FeedingTimes
                    .OrderByDescending(feeding => feeding.FeedingTime)
                    .Select(feeding => feeding.FeedingTime)
                    .FirstOrDefault(), DateTime.UtcNow)
                })
                .ToListAsync();

            return new PagedResult<GetPetDTO>
            {
                Items = items,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Retrieves a specific pet by its unique identifier.
        /// </summary>
        /// <param name="id">The primary key of the pet to retrieve.</param>
        /// <returns>A <see cref="GetPetDTO"/> if found; otherwise, null.</returns>
        public async Task<GetPetDTO?> GetPetByIdAsync(int id)
        {
            var now = DateTime.UtcNow;

            return await context.Pets
                .Where(pet => pet.Id == id)
                .Select(pet => new GetPetDTO
                {
                    Id = pet.Id,
                    Name = pet.Name,
                    Age = pet.Age,
                    AdditionalInformation = pet.AdditionalInformation,
                    CreationDate = pet.CreationDate,
                    RowVersion = pet.RowVersion,

                    LastFed = pet.FeedingTimes
                    .OrderByDescending(feeding => feeding.FeedingTime)
                    .Select(feeding => feeding.FeedingTime)
                    .FirstOrDefault(),

                    Status = CalculateHunger(pet.FeedingTimes
                    .OrderByDescending(feeding => feeding.FeedingTime)
                    .Select(feeding => feeding.FeedingTime)
                    .FirstOrDefault(), now)
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Persists a new pet record and assigns an initial creation timestamp.
        /// </summary>
        /// <param name="petToAdd">The data required to create the pet.</param>
        /// <returns><see cref="GetPetDTO"/> representing the newly created pet, including its generated ID.</returns>
        public async Task<GetPetDTO> AddPetAsync(CommandPetDTO petToAdd)
        {
            var pet = new Pet
            {
                Name = petToAdd.Name,
                Age = petToAdd.Age,
                AdditionalInformation = petToAdd.AdditionalInformation,
                CreationDate = DateTime.UtcNow
            };
            

            context.Pets.Add(pet);
            await context.SaveChangesAsync();

            return new GetPetDTO
            {
                Id = pet.Id,
                Name = pet.Name,
                Age = pet.Age,
                AdditionalInformation = pet.AdditionalInformation,
                CreationDate = pet.CreationDate
            };
        }

        /// <summary>
        /// Updates an existing pet record with concurrency conflict detection.
        /// </summary>
        /// <param name="id">The ID of the pet to update.</param>
        /// <param name="petToOverride">The updated pet data.</param>
        /// <returns>A <see cref="ServiceResult"/> indicating success or the specific cause of failure.</returns>
        public async Task<ServiceResult> OverridePetAsync(int id, CommandPetDTO petToOverride)
        {
            var petRequested = await context.Pets.FindAsync(id);

            if (petRequested == null)
                return ServiceResult.Fail(ServiceResultError.NotFound);

            petRequested.Name = petToOverride.Name;
            petRequested.Age = petToOverride.Age;
            petRequested.AdditionalInformation = petToOverride.AdditionalInformation;

            if (petToOverride.RowVersion != null)
            {
                context.Entry(petRequested).Property(p => p.RowVersion).OriginalValue = petToOverride.RowVersion;
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return ServiceResult.Fail(ServiceResultError.ConcurrencyConflict);
            }

            return ServiceResult.Ok();
        }

        /// <summary>
        /// Removes an existing pet record.
        /// </summary>
        /// <param name="id">The ID of the pet to update.</param>
        /// <returns>A <see cref="bool"/> indicating success or failure.</returns>
        public async Task<bool> DeletePetAsync(int id)
        {
            var petItem = await context.Pets.FindAsync(id);

            if (petItem == null)
                return false;

            context.Pets.Remove(petItem);
            await context.SaveChangesAsync();

            return true;
        }

        private static HungerStatus CalculateHunger(DateTime? lastFed, DateTime now)
        {
            if (lastFed == null) return HungerStatus.Starving;

            var hoursSinceLastFeeding = (now - lastFed.Value).TotalHours;

            return hoursSinceLastFeeding switch
            {
                < 2 => HungerStatus.Full,
                < 5 => HungerStatus.Content,
                < 10 => HungerStatus.Hungry,
                _ => HungerStatus.Starving,
            };
        }
    }
}
