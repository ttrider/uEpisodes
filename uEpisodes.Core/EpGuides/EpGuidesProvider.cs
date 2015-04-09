using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TTRider.uEpisodes.Core.EpGuides
{
    public class EpGuidesProvider
    {
        public async Task GetShowsAsync(Action<IShowInfo> showHandler, CancellationToken cancellationToken)
        {
            if (showHandler == null) throw new ArgumentNullException("showHandler");

            using (var reader = await DataClient.ReadDataAsync(new Uri(@"http://epguides.com/common/allshows.txt"), cancellationToken))
            {
                await DataClient.ProcessRecordsAsync(reader, record => showHandler(
                    new ShowInfo
                    {
                        Title = record[0],
                        Directory = record[1],
                        Country = record[2],
                        TvRage = record[3]
                    }), "title", "directory", "country", "tvrage");
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

                await DataClient.ProcessRecordsAsync(reader, record =>
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
