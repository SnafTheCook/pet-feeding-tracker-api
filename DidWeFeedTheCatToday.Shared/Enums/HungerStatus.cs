using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidWeFeedTheCatToday.Shared.Enums
{
    public enum HungerStatus
    {
        Full,       //Fed less than 2 hours ago
        Content,    //Fed between 2 and 5 hours ago
        Hungry,     //Fed 5-10 hours ago
        Starving    //Fed 10+ hours ago (or 10+ minutes ago for cats)
    }
}
