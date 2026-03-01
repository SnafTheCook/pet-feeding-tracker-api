using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using DidWeFeedTheCatToday.Shared.Enums;

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
