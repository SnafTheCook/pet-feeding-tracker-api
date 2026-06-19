using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using DidWeFeedTheCatToday.Shared.Utils;
using Riok.Mapperly.Abstractions;

namespace DidWeFeedTheCatToday.Mappers
{
    [Mapper]
    public partial class PetMapper
    {
        [MapProperty(nameof(Pet.CreatedAt), nameof(GetPetDTO.CreationDate))]
        [MapperIgnoreTarget(nameof(GetPetDTO.LastFed))]
        [MapperIgnoreTarget(nameof(GetPetDTO.Status))]
        [MapperIgnoreSource(nameof(Pet.IsDeleted))]
        [MapperIgnoreSource(nameof(Pet.LastModifiedAt))]
        [MapperIgnoreSource(nameof(Pet.FeedingTimes))]
        public partial GetPetDTO MapToDtoInternal(Pet pet);

        public GetPetDTO PetToGetPetDTO(Pet pet)
        {
            var dto = MapToDtoInternal(pet);

            var lastFed = pet.FeedingTimes
                .OrderByDescending(f => f.FeedingTime)
                .Select(f => f.FeedingTime)
                .FirstOrDefault();

            dto.LastFed = lastFed;
            dto.Status = PetStatusCalculator.CalculateHunger(lastFed, DateTime.UtcNow);

            return dto;
        }
    }
}
