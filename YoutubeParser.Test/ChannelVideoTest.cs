using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using YoutubeParser.Channels;
using YoutubeParser.ChannelVideos;
using YoutubeParser.Commons;
using YoutubeParser.Test.Utils;
using YoutubeParser.Utils;

namespace YoutubeParser.Test
{
    [TestClass]
    public class ChannelVideoTest
    {
        [TestMethod]
        public void Upcoming_Video()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideo_Upcoming_Video_Html.txt");
            var html = File.ReadAllText(file);
            var pageExtractor = new ChannelVideoPageExtractor(html);
            var channelVideoItem = pageExtractor.GetVideoItems().First();
            var extractor = new ChannelVideoExtractor(channelVideoItem);
            var channelVideo = Map(extractor);
            Assert.IsNotNull(channelVideo);
            Assert.AreEqual(channelVideo.VideoStatus, VideoStatus.Upcoming);
            Assert.AreEqual(channelVideo.VideoType, VideoType.Video);
            Assert.AreEqual(channelVideo.Duration, null);
            Assert.AreEqual(channelVideo.PublishedTime, "");
            //Assert.AreEqual(channelVideo.UpcomingDate, DateTime.Parse(""));
            Assert.AreEqual(channelVideo.IsShorts, false);
        }

        [TestMethod]
        public void Upcoming_Video2()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideo_Upcoming_Video2_Html.txt");
            var html = File.ReadAllText(file);
            var pageExtractor = new ChannelVideoPageExtractor(html);
            var channelVideoItem = pageExtractor.GetVideoItems().Last();
            var extractor = new ChannelVideoExtractor(channelVideoItem);
            var channelVideo = Map(extractor);
            Assert.IsNotNull(channelVideo);
            Assert.AreEqual(channelVideo.VideoStatus, VideoStatus.Upcoming);
            Assert.AreEqual(channelVideo.VideoType, VideoType.Video);
            Assert.AreEqual(channelVideo.Duration, null);
            Assert.AreEqual(channelVideo.PublishedTime, "");
            //Assert.AreEqual(channelVideo.UpcomingDate, DateTime.Parse(""));
            Assert.AreEqual(channelVideo.IsShorts, false);
        }

        [TestMethod]
        public void Live_Video()
        {
        }

        [TestMethod]
        public void Default_Video()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideo_Default_Video_Html.txt");
            var html = File.ReadAllText(file);
            var pageExtractor = new ChannelVideoPageExtractor(html);
            var channelVideoItem = pageExtractor.GetVideoItems().First();
            var extractor = new ChannelVideoExtractor(channelVideoItem);
            var channelVideo = Map(extractor);
            Assert.IsNotNull(channelVideo);
            Assert.AreEqual(channelVideo.VideoStatus, VideoStatus.Default);
            Assert.AreEqual(channelVideo.VideoType, VideoType.Video);
            //Assert.AreEqual(channelVideo.Duration, null);
            //Assert.AreEqual(channelVideo.PublishedTime, "");
            Assert.AreEqual(channelVideo.UpcomingDate, null);
            Assert.AreEqual(channelVideo.IsShorts, false);
        }

        [TestMethod]
        public void Default_Video_Premiered()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideo_Default_Video_Premiered_Html.txt");
            var html = File.ReadAllText(file);
            var pageExtractor = new ChannelVideoPageExtractor(html);
            var channelVideoItem = pageExtractor.GetVideoItems().First();
            var extractor = new ChannelVideoExtractor(channelVideoItem);
            var channelVideo = Map(extractor);
            Assert.IsNotNull(channelVideo);
            Assert.AreEqual(channelVideo.VideoStatus, VideoStatus.Default);
            Assert.AreEqual(channelVideo.VideoType, VideoType.Video);
            //Assert.AreEqual(channelVideo.Duration, null);
            //Assert.AreEqual(channelVideo.PublishedTime, "");
            Assert.AreEqual(channelVideo.UpcomingDate, null);
            Assert.AreEqual(channelVideo.IsShorts, false);
        }

        [TestMethod]
        public void Upcoming_Stream()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideo_Upcoming_Stream_Html.txt");
            var html = File.ReadAllText(file);
            var pageExtractor = new ChannelVideoPageExtractor(html);
            var channelVideoItem = pageExtractor.GetVideoItems().First();
            var extractor = new ChannelVideoExtractor(channelVideoItem);
            var channelVideo = Map(extractor);
            Assert.IsNotNull(channelVideo);
            Assert.AreEqual(channelVideo.VideoStatus, VideoStatus.Upcoming);
            Assert.AreEqual(channelVideo.VideoType, VideoType.Stream);
            Assert.AreEqual(channelVideo.Duration, null);
            Assert.AreEqual(channelVideo.PublishedTime, "");
            //Assert.AreEqual(channelVideo.UpcomingDate, DateTime.Parse(""));
            Assert.AreEqual(channelVideo.IsShorts, false);
        }

        [TestMethod]
        public void Upcoming_Stream2()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideo_Upcoming_Stream2_Html.txt");
            var html = File.ReadAllText(file);
            var pageExtractor = new ChannelVideoPageExtractor(html);
            var channelVideoItem = pageExtractor.GetVideoItems().Last();
            var extractor = new ChannelVideoExtractor(channelVideoItem);
            var channelVideo = Map(extractor);
            Assert.IsNotNull(channelVideo);
            Assert.AreEqual(channelVideo.VideoStatus, VideoStatus.Upcoming);
            Assert.AreEqual(channelVideo.VideoType, VideoType.Stream);
            Assert.AreEqual(channelVideo.Duration, null);
            Assert.AreEqual(channelVideo.PublishedTime, "");
            //Assert.AreEqual(channelVideo.UpcomingDate, DateTime.Parse(""));
            Assert.AreEqual(channelVideo.IsShorts, false);
        }

        [TestMethod]
        public void Live_Stream()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideo_Live_Stream_Html.txt");
            var html = File.ReadAllText(file);
            var pageExtractor = new ChannelVideoPageExtractor(html);
            var channelVideoItem = pageExtractor.GetVideoItems().First();
            var extractor = new ChannelVideoExtractor(channelVideoItem);
            var channelVideo = Map(extractor);
            Assert.IsNotNull(channelVideo);
            Assert.AreEqual(channelVideo.VideoStatus, VideoStatus.Live);
            Assert.AreEqual(channelVideo.VideoType, VideoType.Stream);
            Assert.AreEqual(channelVideo.Duration, null);
            //Assert.AreEqual(channelVideo.PublishedTime, "");
            Assert.AreEqual(channelVideo.UpcomingDate, null);
            Assert.AreEqual(channelVideo.IsShorts, false);
        }

        [TestMethod]
        public void Default_Stream()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideo_Default_Stream_Html.txt");
            var html = File.ReadAllText(file);
            var pageExtractor = new ChannelVideoPageExtractor(html);
            var channelVideoItem = pageExtractor.GetVideoItems().First();
            var extractor = new ChannelVideoExtractor(channelVideoItem);
            var channelVideo = Map(extractor);
            Assert.IsNotNull(channelVideo);
            Assert.AreEqual(channelVideo.VideoStatus, VideoStatus.Default);
            Assert.AreEqual(channelVideo.VideoType, VideoType.Stream);
            //Assert.AreEqual(channelVideo.Duration, null);
            //Assert.AreEqual(channelVideo.PublishedTime, "");
            Assert.AreEqual(channelVideo.UpcomingDate, null);
            Assert.AreEqual(channelVideo.IsShorts, false);
        }

        internal ChannelVideo Map(ChannelVideoExtractor extractor)
        {
            return new ChannelVideo
            {
                Title = extractor.GetTitle(),
                VideoId = extractor.GetVideoId(),
                Thumbnails = extractor.GetThumbnails(),
                RichThumbnail = extractor.TryGetRichThumbnail(),
                ViewCount = extractor.GetViewCount(),
                Duration = extractor.TryGetDuration(),
                PublishedTime = extractor.GetPublishedTime(),
                PublishedTimeSeconds = extractor.GetPublishedTimeSeconds(),
                UpcomingDate = extractor.TryGetUpcomingDate(),
                VideoStatus = extractor.GetVideoStatus(),
                VideoType = extractor.GetVideoType(),
                IsShorts = extractor.IsShorts()
            };
        }
    }
}