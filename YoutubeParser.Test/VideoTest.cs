using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeParser.Commons;
using YoutubeParser.Videos;

namespace YoutubeParser.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Upcoming_Video()
        {
            var file = Path.Combine(TestFile.Path, "Video_Upcoming_Video_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new VideoPageExtractor(html);
            var video = new Video
            {
                VideoType = extractor.GetVideoType(),
                VideoStatus = extractor.GetVideoStatus()
            };
            Assert.IsNotNull(video);
            Assert.AreEqual(video.VideoType, VideoType.Video);
            Assert.AreEqual(video.VideoStatus, VideoStatus.Upcoming);
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