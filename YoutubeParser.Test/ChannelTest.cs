using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeParser.Channels;
using YoutubeParser.Commons;
using YoutubeParser.Test.Utils;
using YoutubeParser.Videos;

namespace YoutubeParser.Test
{
    [TestClass]
    public class ChannelTest
    {
        [TestMethod]
        public void Info()
        {
            var file = Path.Combine(TestFile.DirPath, "Channel.txt");
            var html = File.ReadAllText(file);
            var extractor = new ChannelPageExtractor(html);
            var channel = Map(extractor);
            Assert.IsNotNull(channel);
            Assert.AreEqual(channel.JoinedDate, DateTime.Parse("2021/6/5"));
            Assert.AreEqual(channel.SubscriberCount, 182000);
            Assert.AreEqual(channel.ViewCount, 14433414);
        }

        internal Channel Map(ChannelPageExtractor extractor)
        {
            return new Channel
            {
                Title = extractor.GetTitle(),
                ChannelId = extractor.GetChannelId(),
                Description = extractor.GetDescription(),
                CanonicalChannelUrl = extractor.GetCanonicalChannelUrl(),
                Country = extractor.GetCountry(),
                SubscriberCount = extractor.GetSubscriberCount(),
                ViewCount = extractor.GetViewCount(),
                JoinedDate = extractor.GetJoinedDate(),
                Thumbnails = extractor.GetThumbnails(),
                Banners = extractor.GetBanners(),
            };
        }
    }
}