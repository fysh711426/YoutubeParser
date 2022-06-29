using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using YoutubeParser.Channels;
using YoutubeParser.ChannelVideos;
using YoutubeParser.Commons;
using YoutubeParser.Utils;

namespace YoutubeParser.Test
{
    [TestClass]
    public class ChannelVideoTest
    {
        [TestMethod]
        public void Upcoming_Video()
        {
            var file = Path.Combine(TestFile.Path, "ChannelVideo_Upcoming_Video_Html.txt");
            var html = File.ReadAllText(file);
            var parser = new YoutubeChannelParser(Http.Client);
            var mapVideo = parser.GetType().GetMethod("MapVideo", BindingFlags.Instance | BindingFlags.NonPublic);
            var extractor = new ChannelVideoPageExtractor(html);
            var channelVideoItem = extractor.GetVideoItems().FirstOrDefault();
            var channelVideo = (ChannelVideo?)mapVideo?.Invoke(parser, new[] { channelVideoItem });
            Assert.IsNotNull(channelVideo);
            Assert.AreEqual(channelVideo.VideoType, VideoType.Video);
            Assert.AreEqual(channelVideo.VideoStatus, VideoStatus.Upcoming);
        }

        [TestMethod]
        public void Live_Video()
        {
        }

        [TestMethod]
        public void Default_Video()
        {
        }

        [TestMethod]
        public void Upcoming_Stream()
        {
        }

        [TestMethod]
        public void Live_Stream()
        {
        }

        [TestMethod]
        public void Default_Stream()
        {
        }
    }
}