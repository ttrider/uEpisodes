using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TTRider.uEpisodes.TVDatabase
{
    class Shows : DataItem
    {
        public static Shows Current = new Shows();

        List<Show> shows;

        public Shows()
        {
            this.CustomMappings = new CustomMappingCollection();
        }

        public IEnumerable<string> ShowList
        {
            get { return GetShows(new CancellationToken(false)).Select(s => s.Title); }
        }

        public CustomMappingCollection CustomMappings
        {
            get;
            private set;
        }


        public List<Show> GetShows(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            lock (this.Sync)
            {
                var sf = FileData.Get(new Uri(@"http://epguides.com/common/allshows.txt"), token);
                if (sf.FromCache)
                {
                    if (this.shows != null)
                    {
                        return this.shows;
                    }
                }

                this.shows =
                    new List<Show>(
                        EnumerateRecords(sf.Data, "title", "directory", "tvrage", "country").Select(item => new Show
                            {
                                Title = item[0],
                                Directory = item[1],
                                TvRage = item[2],
                                Country = item[3]
                            }));
                return this.shows;
            }
        }

        public Episode[] GetShowEpisode(string originalShowName, int season, int episode, CancellationToken token)
        {
            var episodes = new List<Episode>();
            var list = GetShows(token);

            var showWords = FileData.GetWordset(originalShowName).ToList();
            var showName = string.Join(" ", showWords);

            Show exactShow = null;

            if (this.CustomMappings.Contains(showName))
            {
                var custom = this.CustomMappings[showName];
                exactShow =
                    list.FirstOrDefault(s => s.Title.Equals(custom.ShowName, StringComparison.CurrentCultureIgnoreCase));
            }

            if (exactShow == null)
            {
                exactShow = list.FirstOrDefault(s => s.NormalizedTitle.Equals(showName)) ??
                            list.FirstOrDefault(
                                s => s.Directory.Equals(showName, StringComparison.CurrentCultureIgnoreCase)) ??
                            list.FirstOrDefault(
                                s => s.Title.Equals(showName, StringComparison.CurrentCultureIgnoreCase));
            }

            if (exactShow == null)
            {
                var ss = new HashSet<string>(showWords);

                foreach (var sh in list.Where(s => ss.IsSubsetOf(s.ExtendedWordset)))
                {
                    Episode ep;
                    if (!sh.TryGetEpisode(token, season, episode, out ep))
                    {
                        ep = new Episode(sh.Title, season.ToString(CultureInfo.InvariantCulture),
                                         episode.ToString(CultureInfo.InvariantCulture), "", false);
                    }
                    episodes.Add(ep);
                }
            }
            else
            {
                Episode episodeItem;
                if (exactShow.TryGetEpisode(token, season, episode, out episodeItem))
                {
                    episodes.Add(episodeItem);
                }

            }
            return episodes.ToArray();
        }
    }
}
