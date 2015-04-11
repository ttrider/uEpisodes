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

        private HashSet<string> wordset; 
        
        public double Match(IEnumerable<string> words)
        {
            if (wordset == null)
            {
                //wordset = new HashSet<string>(DataClient.GetWordset(Title + " " + Country));
                wordset = new HashSet<string>(DataClient.GetWordset(Title));
            }

            var count = 0;
            var index = 0;
            foreach (var word in words)
            {
                index++;
                if (wordset.Contains(word))
                {
                    count++;
                }
            }
            if (index == 0) return 0.0;
            return (double) count/(double) index;
        }
    }
}
