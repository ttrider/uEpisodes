using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TTRider.uEpisodes.Core.Discovery;

namespace Tests
{
    [TestClass]
    public class DiscoveryTests
    {
        [TestMethod]
        public void FPPCtor()
        {
            var fpp = new FilePathProcessor();
            Assert.IsNotNull(fpp);
        }

        [TestMethod]
        public void Initialize00()
        {
            var fpp = new FilePathProcessor();
            Assert.IsNotNull(fpp);
            fpp.Initialize(null, null);
        }


        [TestMethod]
        public void SimpleEnd2End()
        {
            var fpp = new FilePathProcessor();
            Assert.IsNotNull(fpp);
            fpp.Initialize(new[] { @"^(?'show'.*)S\s*(?'season'\d+)\s*E\s*(?'episode'\d+)" }, null);

            var result = fpp.ProcessFile(@"MyShow\Season 1\MyShow S01E10 - something.avi");
            Assert.IsNotNull(result);
            Assert.AreEqual(1,result.VideoIdSet.Count);

            Assert.AreEqual("MyShow",result.VideoIdSet[0].Show);
            Assert.AreEqual(1, result.VideoIdSet[0].Season.GetValueOrDefault());
            Assert.AreEqual(10, result.VideoIdSet[0].Episode.GetValueOrDefault());
        }

        [TestMethod]
        public void SimpleExclude()
        {
            var fpp = new FilePathProcessor();
            Assert.IsNotNull(fpp);
            fpp.Initialize(null, new []{"Sample"});

            var result = fpp.ProcessFile(@"MyShow\Sample\MyShow S01E10 - something.avi");
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.VideoIdSet.Count);

        }

    }
}
