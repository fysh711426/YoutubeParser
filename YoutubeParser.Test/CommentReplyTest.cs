using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeParser.Comments;
using YoutubeParser.Test.Utils;

namespace YoutubeParser.Test
{
    [TestClass]
    public class CommentReplyTest
    {
        [TestMethod]
        public void VideoReplies()
        {
            var file = Path.Combine(TestFile.DirPath, "CommentTest_VideoReplies.txt");
            var json = File.ReadAllText(file);
            var extractor = new CommentPageExtractor(json);
            var commentItems = extractor.GetReplyItemsFromNext().ToList();
            var continuation = extractor.TryGetReplyContinuation();

            var replies = MapList(commentItems);
            var second = replies.Skip(1).First();

            Assert.IsNotNull(continuation);
            Assert.AreEqual(replies.Count, 10);
            Assert.AreEqual(second.LikeCount, 7);
            Assert.AreEqual(second.AuthorIsChannelOwner, true);
            Assert.AreNotEqual(second.CommentId, "");
            Assert.AreNotEqual(second.AuthorChannelId, "");
        }

        [TestMethod]
        public void CommunityReplies()
        {
            var file = Path.Combine(TestFile.DirPath, "CommentTest_CommunityReplies.txt");
            var json = File.ReadAllText(file);
            var extractor = new CommentPageExtractor(json);
            var commentItems = extractor.GetReplyItemsFromNext().ToList();
            var continuation = extractor.TryGetReplyContinuation();

            var replies = MapList(commentItems);
            var second = replies.Skip(1).First();

            Assert.IsNotNull(continuation);
            Assert.AreEqual(replies.Count, 9);
            Assert.AreEqual(second.LikeCount, 24);
            Assert.AreEqual(second.AuthorIsChannelOwner, false);
            Assert.AreNotEqual(second.CommentId, "");
            Assert.AreNotEqual(second.AuthorChannelId, "");
        }

        internal List<CommentReply> MapList(List<JToken> commentItems)
        {
            var comments = new List<CommentReply>();
            foreach (var item in commentItems)
            {
                comments.Add(Map(item));
            }
            return comments;
        }

        internal CommentReply Map(JToken content)
        {
            var extractor = new CommentExtractor(content);
            return new CommentReply
            {
                CommentId = extractor.GetCommentId(),
                Content = extractor.GetContent(),
                PublishedTime = extractor.GetPublishedTime(),
                PublishedTimeSeconds = extractor.GetPublishedTimeSeconds(),
                LikeCount = extractor.GetLikeCount(),
                AuthorTitle = extractor.GetAuthorTitle(),
                AuthorChannelId = extractor.GetAuthorChannelId(),
                AuthorThumbnails = extractor.GetAuthorThumbnails(),
                AuthorIsChannelOwner = extractor.GetAuthorIsChannelOwner()
            };
        }
    }
}
