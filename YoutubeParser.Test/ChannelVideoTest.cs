using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeParser.ChannelVideos;
using YoutubeParser.Shares;
using YoutubeParser.Test.Utils;

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
            Assert.AreEqual(channelVideo.UpcomingDate, DateTime.Parse("2022/7/1 19:00:00"));
            Assert.AreEqual(channelVideo.IsShorts, false);
            Assert.AreEqual(channelVideo.ViewCount, 27);
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
            Assert.AreEqual(channelVideo.UpcomingDate, DateTime.Parse("2024/6/8 06:00:00"));
            Assert.AreEqual(channelVideo.IsShorts, false);
            Assert.AreEqual(channelVideo.ViewCount, 0);
        }

        [TestMethod]
        public void Live_Video()
        {
            var file = Path.Combine(TestFile.DirPath, "ChannelVideo_Live_Video_Html.txt");
            var html = File.ReadAllText(file);
            var pageExtractor = new ChannelVideoPageExtractor(html);
            var channelVideoItem = pageExtractor.GetVideoItems().Skip(4).First();
            var extractor = new ChannelVideoExtractor(channelVideoItem);
            var channelVideo = Map(extractor);
            Assert.IsNotNull(channelVideo);
            Assert.AreEqual(channelVideo.VideoStatus, VideoStatus.Live);
            Assert.AreEqual(channelVideo.VideoType, VideoType.Video);
            Assert.AreEqual(channelVideo.Duration, null);
            Assert.AreEqual(channelVideo.PublishedTime, "");
            Assert.AreEqual(channelVideo.UpcomingDate, null);
            Assert.AreEqual(channelVideo.IsShorts, false);
            Assert.AreEqual(channelVideo.ViewCount, 5);
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
            Assert.AreEqual(channelVideo.Duration, TimeSpan.Parse("00:10:32"));
            Assert.AreNotEqual(channelVideo.PublishedTime, "");
            Assert.AreEqual(channelVideo.UpcomingDate, null);
            Assert.AreEqual(channelVideo.IsShorts, false);
            Assert.AreEqual(channelVideo.ViewCount, 8181);
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
            Assert.AreEqual(channelVideo.Duration, TimeSpan.Parse("00:08:40"));
            Assert.AreNotEqual(channelVideo.PublishedTime, "");
            Assert.AreEqual(channelVideo.UpcomingDate, null);
            Assert.AreEqual(channelVideo.IsShorts, false);
            Assert.AreEqual(channelVideo.ViewCount, 2250156);
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
            Assert.AreEqual(channelVideo.UpcomingDate, DateTime.Parse("2023/6/12 23:00:00"));
            Assert.AreEqual(channelVideo.IsShorts, false);
            Assert.AreEqual(channelVideo.ViewCount, 1);
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
            Assert.AreEqual(channelVideo.UpcomingDate, DateTime.Parse("2024/6/1 05:45:00"));
            Assert.AreEqual(channelVideo.IsShorts, false);
            Assert.AreEqual(channelVideo.ViewCount, 0);
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
            Assert.AreEqual(channelVideo.PublishedTime, "");
            Assert.AreEqual(channelVideo.UpcomingDate, null);
            Assert.AreEqual(channelVideo.IsShorts, false);
            Assert.AreEqual(channelVideo.ViewCount, 116);
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
            Assert.AreEqual(channelVideo.Duration, TimeSpan.Parse("01:57:21"));
            Assert.AreNotEqual(channelVideo.PublishedTime, "");
            Assert.AreEqual(channelVideo.UpcomingDate, null);
            Assert.AreEqual(channelVideo.IsShorts, false);
            Assert.AreEqual(channelVideo.ViewCount, 848);
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