using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTRider.uEpisodes.Core.EpGuides
{
    public class EpisodeInfo : IEpisodeInfo
    {
        public string Show { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public string Title { get; set; }

    }
}
