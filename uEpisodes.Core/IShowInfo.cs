using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTRider.uEpisodes.Core
{
    public interface IShowInfo
    {
        string Title { get; set; }

        double Match(IEnumerable<string> words);
    }
}
