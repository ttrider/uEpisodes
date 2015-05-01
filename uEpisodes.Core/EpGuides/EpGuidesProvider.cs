using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TTRider.uEpisodes.Core.EpGuides
{
    public class EpGuidesProvider
    {
        static ObjectCache cache = new MemoryCache("EpGuides");
        private static readonly string showSetKey = Guid.NewGuid().ToString("N");
        private static readonly string epPrefixKey = Guid.NewGuid().ToString("N");


        public async Task<IList<IShowInfo>> GetShowListAsync(CancellationToken cancellationToken)
        {
            var showset = cache.Get(showSetKey) as IList<IShowInfo>;
            if (showset == null)
            {
                showset = new List<IShowInfo>();

                await GetShowsAsync((s) => showset.Add(s), cancellationToken);

                cache.Set(showSetKey, showset, DateTimeOffset.Now.AddHours(8));
            }
            return showset;
        }


        public async Task<IList<IEpisodeInfo>> GetEpisodeListAsync(IShowInfo showInfo, CancellationToken cancellationToken)
        {
            var epkey = epPrefixKey + showInfo.Title;

            var epset = cache.Get(epkey) as IList<IEpisodeInfo>;
            if (epset == null)
            {
                epset = new List<IEpisodeInfo>();

                await GetEpisodesAsync(showInfo, (s) => epset.Add(s), cancellationToken);

                cache.Set(epkey, epset, DateTimeOffset.Now.AddHours(8));
            }
            return epset;
        }


        public async Task GetShowsAsync(Action<IShowInfo> showHandler, CancellationToken cancellationToken)
        {
            if (showHandler == null) throw new ArgumentNullException("showHandler");

            using (var reader = await DataClient.ReadDataAsync(new Uri(@"http://epguides.com/common/allshows.txt"), cancellationToken))
            {
                await DataClient.ProcessRecordsAsync(reader, null, record =>
                {
                    var si = new ShowInfo
                    {
                        Title = record[0],
                        Directory = record[1],
                        Country = record[2],
                        TvRage = record[3]
                    };
                    if (!string.IsNullOrWhiteSpace(si.TvRage) && !string.IsNullOrWhiteSpace(si.Title))
                    {
                        showHandler(si);
                    }
                }, "title", "directory", "country", "tvrage");
            }
        }

        public async Task GetEpisodesAsync(IShowInfo show, Action<IEpisodeInfo> episodeHandler,
            CancellationToken cancellationToken)
        {
            if (show == null) throw new ArgumentNullException("show");
            if (episodeHandler == null) throw new ArgumentNullException("episodeHandler");
            var epShow = show as ShowInfo;
            if (epShow == null) throw new ArgumentException("invalid show object type", "show");

            using (
                var reader =
                    await
                        DataClient.ReadDataAsync(
                            new Uri(@"http://epguides.com/common/exportToCSV.asp?rage=" + epShow.TvRage),
                            cancellationToken))
            {
                // filter our <pre> and </pre>
                await DataClient.SkipUntilAsync(reader, "<pre>");

                await DataClient.ProcessRecordsAsync(reader, "</pre>", record =>
                {
                    int season;
                    int.TryParse(record[0], out season);
                    int episode;
                    int.TryParse(record[1], out episode);

                    episodeHandler(
                        new EpisodeInfo
                        {
                            Show = show.Title,
                            Season = season,
                            Episode = episode,
                            Title = record[2]
                        });
                }, "season", "episode", "title");
            }
        }
    }
}
