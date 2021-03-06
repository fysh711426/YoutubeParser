using Microsoft.VisualStudio.TestTools.UnitTesting;
using YoutubeParser.Comments;
using YoutubeParser.Communitys;
using YoutubeParser.Test.Utils;

namespace YoutubeParser.Test
{
    [TestClass]
    public class CommunityTest
    {
        [TestMethod]
        public void Info()
        {
            var file = Path.Combine(TestFile.DirPath, "Community_Html.txt");
            var html = File.ReadAllText(file);
            var pageExtractor = new CommunityPageExtractor(html);
            var communityItem = pageExtractor.GetCommunityItems().First();
            var extractor = new CommunityExtractor(communityItem);
            var community = Map(extractor);
            Assert.IsNotNull(community);
            Assert.AreEqual(community.PublishedTime, "3 days ago");
            Assert.AreEqual(community.LikeCount, 1400);
        }

        internal Community Map(CommunityExtractor extractor)
        {
            return new Community
            {
                PostId = extractor.GetPostId(),
                AuthorTitle = extractor.GetAuthorTitle(),
                AuthorChannelId = extractor.GetAuthorChannelId(),
                AuthorThumbnails = extractor.GetAuthorThumbnails(),
                Content = extractor.GetContent(),
                Images = extractor.GetImages(),
                PublishedTime = extractor.GetPublishedTime(),
                PublishedTimeSeconds = extractor.GetPublishedTimeSeconds(),
                LikeCount = extractor.GetLikeCount()
                //VoteStatus = extractor.GetVoteStatus()
            };
        }
    }
}