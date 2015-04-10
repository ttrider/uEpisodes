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


        public async Task<EpisodeFileInfo> ProcessFile(string path)
        {
            var info =  new EpisodeFileInfo
            {
                VideoFile = fileProcessor.ProcessFile(path)
            };

            if (info.VideoFile.VideoIdSet.Any())
            {
                foreach (var videoId in info.VideoFile.VideoIdSet)
                {

                    
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
