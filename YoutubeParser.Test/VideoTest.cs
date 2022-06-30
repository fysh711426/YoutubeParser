using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeParser.Commons;
using YoutubeParser.Test.Utils;
using YoutubeParser.Videos;

namespace YoutubeParser.Test
{
    [TestClass]
    public class VideoTest
    {
        [TestMethod]
        public void Upcoming_Video()
        {
            var file = Path.Combine(TestFile.DirPath, "Video_Upcoming_Video_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new VideoPageExtractor(html);
            var video = Map(extractor);
            Assert.IsNotNull(video);
            Assert.AreEqual(video.VideoStatus, VideoStatus.Upcoming);
            Assert.AreEqual(video.VideoType, VideoType.Video);
            Assert.AreEqual(video.Duration, null);
            //Assert.AreEqual(video.UploadDate, "");
        }

        [TestMethod]
        public void Live_Video()
        {
        }

        [TestMethod]
        public void Default_Video()
        {
            var file = Path.Combine(TestFile.DirPath, "Video_Default_Video_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new VideoPageExtractor(html);
            var video = Map(extractor);
            Assert.IsNotNull(video);
            Assert.AreEqual(video.VideoStatus, VideoStatus.Default);
            Assert.AreEqual(video.VideoType, VideoType.Video);
            //Assert.AreEqual(video.Duration, null);
            //Assert.AreEqual(video.UploadDate, "");
        }

        [TestMethod]
        public void Default_Video_Premiered()
        {
            var file = Path.Combine(TestFile.DirPath, "Video_Default_Video_Premiered_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new VideoPageExtractor(html);
            var video = Map(extractor);
            Assert.IsNotNull(video);
            Assert.AreEqual(video.VideoStatus, VideoStatus.Default);
            Assert.AreEqual(video.VideoType, VideoType.Video);
            //Assert.AreEqual(video.Duration, null);
            //Assert.AreEqual(video.UploadDate, "");
        }

        [TestMethod]
        public void Upcoming_Stream()
        {
            var file = Path.Combine(TestFile.DirPath, "Video_Upcoming_Stream_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new VideoPageExtractor(html);
            var x = extractor.TryGetInitialData();
            var video = Map(extractor);
            Assert.IsNotNull(video);
            Assert.AreEqual(video.VideoStatus, VideoStatus.Upcoming);
            Assert.AreEqual(video.VideoType, VideoType.Stream);
            Assert.AreEqual(video.Duration, null);
            //Assert.AreEqual(video.UploadDate, "");
        }

        [TestMethod]
        public void Live_Stream()
        {
            var file = Path.Combine(TestFile.DirPath, "Video_Live_Stream_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new VideoPageExtractor(html);
            var video = Map(extractor);
            Assert.IsNotNull(video);
            Assert.AreEqual(video.VideoStatus, VideoStatus.Live);
            Assert.AreEqual(video.VideoType, VideoType.Stream);
            Assert.AreEqual(video.Duration, null);
            //Assert.AreEqual(video.UploadDate, "");
        }

        [TestMethod]
        public void Default_Stream()
        {
            var file = Path.Combine(TestFile.DirPath, "Video_Default_Stream_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new VideoPageExtractor(html);
            var video = Map(extractor);
            Assert.IsNotNull(video);
            Assert.AreEqual(video.VideoStatus, VideoStatus.Default);
            Assert.AreEqual(video.VideoType, VideoType.Stream);
            //Assert.AreEqual(video.Duration, null);
            //Assert.AreEqual(video.UploadDate, "");
        }

        internal Video Map(VideoPageExtractor extractor)
        {
            return new Video
            {
                VideoId = extractor.GetVideoId(),
                Title = extractor.GetTitle(),
                Description = extractor.GetDescription(),
                AuthorTitle = extractor.GetAuthorTitle(),
                AuthorChannelId = extractor.GetAuthorChannelId(),
                Duration = extractor.TryGetDuration(),
                Thumbnails = extractor.GetThumbnails(),
                Keywords = extractor.GetKeywords(),
                ViewCount = extractor.GetViewCount(),
                UploadDate = extractor.GetUploadDate(),
                LikeCount = extractor.TryGetLikeCount(),
                IsPrivate = extractor.IsPrivate(),
                IsPlayable = extractor.IsPlayable(),
                VideoType = extractor.GetVideoType(),
                VideoStatus = extractor.GetVideoStatus()
            };
        }
    }
}