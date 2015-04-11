using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TTRider.uEpisodes.Core.EpGuides;
using TTRider.uEpisodes.Core.Properties;

namespace TTRider.uEpisodes.Core.Discovery
{
    public class EpisodeDiscoveryProcessor
    {
        private readonly FilePathProcessor fileProcessor = new FilePathProcessor();
        private readonly EpGuidesProvider dataProvider = new EpGuidesProvider();


        public void Initialize(Settings settings)
        {
            this.fileProcessor.Initialize(settings.GetFileNamePatterns(), settings.GetIgnoredDirectories());
        }


        public async Task<EpisodeFileInfo> ProcessFile(string path, CancellationToken cancellationToken)
        {
            var info = new EpisodeFileInfo
            {
                VideoFile = fileProcessor.ProcessFile(path)
            };


            var showSet = await dataProvider.GetShowListAsync(cancellationToken);


            foreach (var videoId in info.GetVideoIdCandidates())
            {
                var wordset = DataClient.GetWordset(videoId.Show).ToList();

                if (wordset.Count > 0 && videoId.Season.HasValue && videoId.Episode.HasValue)
                {
                    var season = videoId.Season.Value;
                    var episode = videoId.Episode.Value;
                    Console.WriteLine("{0}-{1}-{2}", season, episode, videoId.Show);

                    foreach (var showRank in
                        showSet
                            .Select(ss => new { ShowInfo = ss, Match = ss.Match(wordset) })
                            .OrderByDescending(ss => ss.Match)
                            .Where(ss => ss.Match > 0)
                        )
                    {

                        Console.WriteLine("-- {0},{1}",showRank.ShowInfo.Title, showRank.Match);

                        // var episodes = await dataProvider.GetEpisodeListAsync(showRank.ShowInfo, cancellationToken);
                        //var episodeInfo = episodes.FirstOrDefault(ep => ep.Season == season && ep.Episode == episode);


                    }
                }
            }

            return info;
        }






        public EpisodeFileInfo Refine(EpisodeFileInfo info)
        {
            return info;
        }


        //IEnumerable<IShowInfo> LookupShow(string show)
        //{
        //    if (string.IsNullOrWhiteSpace(show)) yield break;


        //    List<IShowInfo> shows = new List<IShowInfo>();

        //    dataProvider.GetShowsAsync((showInfo) =>
        //    {
        //        shows.Add(showInfo);

        //    }, CancellationToken.None).Wait();

        //    return shows;

        //}





    }
}
