using DidWeFeedTheCatToday.Common;
using DidWeFeedTheCatToday.DTOs.Pets;

namespace DidWeFeedTheCatToday.Services.Interfaces
{
    public interface IPetService
    {
        Task<IEnumerable<GetPetDTO>> GetAllPetsAsync();
        Task<GetPetDTO?> GetPetByIdAsync(int id);
        Task<GetPetDTO> AddPetAsync(CommandPetDTO petToAdd);
        Task<ServiceResult> OverridePetAsync(int id, CommandPetDTO petToOverride);
        Task<bool> DeletePetAsync(int id);
    }
}
