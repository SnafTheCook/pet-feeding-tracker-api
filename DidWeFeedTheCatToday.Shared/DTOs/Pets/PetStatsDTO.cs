using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidWeFeedTheCatToday.Shared.DTOs.Pets
{
    public record PetStatsDTO
    (
        int TotalPets,
        int HungryPets,
        int FedTodayCount
    );
}
