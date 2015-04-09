using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTRider.uEpisodes.Core
{
    public interface IEpisodeInfo
    {
        string Show { get; }
        int Season { get; }
        int Episode { get; }
        string Title { get; }

    }
}
