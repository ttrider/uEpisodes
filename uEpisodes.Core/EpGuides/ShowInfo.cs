using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TTRider.uEpisodes.Core.EpGuides
{
    public class ShowInfo : IShowInfo
    {
        public string Title { get; set; }
        public string Directory { get; set; }
        public string TvRage { get; set; }
        public string Country { get; set; }
    }
}
