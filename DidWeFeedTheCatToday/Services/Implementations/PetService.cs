using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;

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
            return await context.Pets
                .AsNoTracking()
                .Select(p => PetToGetPetDTO(p))
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a specific pet by its unique identifier.
        /// </summary>
        /// <param name="id">The primary key of the pet to retrieve.</param>
        /// <returns>A <see cref="GetPetDTO"/> if found; otherwise, null.</returns>
        public async Task<GetPetDTO?> GetPetByIdAsync(int id)
        {
            var pet = await context.Pets.FindAsync(id);

            if (pet == null)
                return null;

            return PetToGetPetDTO(pet);
        }

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

            return PetToGetPetDTO(pet);
        }

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

        public async Task<bool> DeletePetAsync(int id)
        {
            var petItem = await context.Pets.FindAsync(id);

            if (petItem == null)
                return false;

            context.Pets.Remove(petItem);
            await context.SaveChangesAsync();

            return true;
        }

        private static GetPetDTO PetToGetPetDTO(Pet pet) =>
            new GetPetDTO
            {
                Id = pet.Id,
                Name = pet.Name,
                Age = pet.Age,
                AdditionalInformation = pet.AdditionalInformation,
                CreationDate = pet.CreationDate
            };
    }
}
