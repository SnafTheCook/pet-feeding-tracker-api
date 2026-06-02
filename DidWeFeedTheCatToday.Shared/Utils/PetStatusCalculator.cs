using DidWeFeedTheCatToday.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidWeFeedTheCatToday.Shared.Utils
{
    public static class PetStatusCalculator
    {
        public static HungerStatus CalculateHunger(DateTime? lastFed, DateTime now)
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
