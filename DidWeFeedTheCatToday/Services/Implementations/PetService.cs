using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Features.Pets;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using DidWeFeedTheCatToday.Shared.Enums;
using DidWeFeedTheCatToday.Shared.Utils;
using MassTransit.Futures.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace DidWeFeedTheCatToday.Services.Implementations
{
    public class PetService(AppDbContext context, IMemoryCache cache) : IPetService
    {
        private static CancellationTokenSource _resetCacheToken = new();

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

            PetCacheHelper.InvalidateCache();

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

            petItem.MarkAsDeleted();
            await context.SaveChangesAsync();

            PetCacheHelper.InvalidateCache();

            return true;
        }

        public async Task<bool> RestorePetAsync(int id)
        {
            var pet = await context.Pets
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pet == null) 
                return false;

            pet.Restore();
            await context.SaveChangesAsync();

            PetCacheHelper.InvalidateCache();

            return true;
        }
    }
}
