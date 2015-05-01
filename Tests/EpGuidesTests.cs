using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTRider.uEpisodes.Core;
using TTRider.uEpisodes.Core.EpGuides;

namespace Tests
{
    [TestClass]
    public class EpGuidesTests
    {
        [TestMethod]
        public async Task GetShows()
        { 
            var epg = new EpGuidesProvider();

            var shows = new List<IShowInfo>();
            await epg.GetShowsAsync(shows.Add, CancellationToken.None);


            var episodes = new List<IEpisodeInfo>();
            await epg.GetEpisodesAsync(shows.First(), episodes.Add, CancellationToken.None);

        }

        [TestMethod]
        public async Task GetEpisodes()
        {
            var stream = await DataClient.ReadDataAsync(new Uri(@"http://epguides.com/common/exportToCSV.asp?rage=32656"), CancellationToken.None);

            await DataClient.SkipUntilAsync(stream, "<pre>");

            var buffer = await stream.ReadToEndAsync();


        }



        //
    }
}
