using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;

namespace DidWeFeedTheCatToday.Services.Interfaces
{
    public interface IPetService
    {
        Task<ServiceResult> OverridePetAsync(int id, CommandPetDTO petToOverride);
        Task<bool> DeletePetAsync(int id);
        Task<bool> RestorePetAsync(int id);
    }
}
