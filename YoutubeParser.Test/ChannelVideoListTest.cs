using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Reflection;
using YoutubeParser.Channels;
using YoutubeParser.ChannelVideos;
using YoutubeParser.Commons;
using YoutubeParser.Test.Utils;
using YoutubeParser.Utils;

namespace YoutubeParser.Test
{
    [TestClass]
    public class ChannelVideoListTest
    {
        [TestMethod]
        public void UpcomingLiveStreams()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideoList_UpcomingLive_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new ChannelVideoPageExtractor(html);
            var videoItems = extractor.GetVideoItems().ToList();
            var continuation = extractor.TryGetContinuation();
            Assert.AreEqual(videoItems.Count, 1);
            Assert.IsNull(continuation);
        }

        [TestMethod]
        public void UpcomingLiveStreams_Empty()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideoList_UpcomingLive_Empty_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new ChannelVideoPageExtractor(html);
            var videoItems = new List<JToken>();
            var subMenuTitle = extractor.GetSelectedSubMenuTitle();
            if (subMenuTitle == "Upcoming live streams")
                videoItems = extractor.GetVideoItems().ToList();
            var continuation = extractor.TryGetContinuation();
            Assert.AreEqual(videoItems.Count, 0);
            Assert.IsNull(continuation);
        }

        [TestMethod]
        public void LiveStreams()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideoList_Live_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new ChannelVideoPageExtractor(html);
            var videoItems = extractor.GetVideoItems().ToList();
            var continuation = extractor.TryGetContinuation();
            Assert.AreEqual(videoItems.Count, 30);
            Assert.IsNotNull(continuation);
        }

        [TestMethod]
        public void LiveStreams_Empty()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideoList_Live_Empty_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new ChannelVideoPageExtractor(html);
            var videoItems = new List<JToken>();
            var subMenuTitle = extractor.GetSelectedSubMenuTitle();
            if (subMenuTitle == "Live now")
                videoItems = extractor.GetVideoItems().ToList();
            var continuation = extractor.TryGetContinuation();
            Assert.AreEqual(videoItems.Count, 0);
            Assert.IsNull(continuation);
        }
    }
}