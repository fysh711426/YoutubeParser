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
    public class CommentTest
    {
        [TestMethod]
        public void SuperThanks()
        {

        }

        [TestMethod]
        public void VideoCommentsReload()
        {
            var file = Path.Combine(TestFile.DirPath, "CommentTest_VideoCommentsReload.txt");
            var json = File.ReadAllText(file);
            var extractor = new CommentPageExtractor(json);
            var commentItems = extractor.GetCommentItemsFromNext().ToList();
            var continuation = extractor.TryGetContinuation();

            var comments = MapList(commentItems);
            var first = comments.First();
            
            Assert.IsNotNull(continuation);
            Assert.AreEqual(comments.Count, 20);
            Assert.AreEqual(first.IsPinned, true);
            Assert.AreEqual(first.AuthorIsChannelOwner, true);
            Assert.AreEqual(first.ReplyCount, 29);
            Assert.AreNotEqual(first.CommentId, "");
            Assert.AreNotEqual(first.AuthorChannelId, "");
        }

        [TestMethod]
        public void VideoCommentsAppend()
        {
            var file = Path.Combine(TestFile.DirPath, "CommentTest_VideoCommentsAppend.txt");
            var json = File.ReadAllText(file);
            var extractor = new CommentPageExtractor(json);
            var commentItems = extractor.GetCommentItemsFromNext().ToList();
            var continuation = extractor.TryGetContinuation();

            var comments = MapList(commentItems);
            var second = comments.Skip(1).First();

            Assert.IsNotNull(continuation);
            Assert.AreEqual(comments.Count, 20);
            Assert.AreEqual(second.IsPinned, false);
            Assert.AreEqual(second.AuthorIsChannelOwner, false);
            Assert.AreEqual(second.ReplyCount, 1);
            Assert.AreNotEqual(second.CommentId, "");
            Assert.AreNotEqual(second.AuthorChannelId, "");
        }

        [TestMethod]
        public void VideoCommentsEmpty()
        {
            var file = Path.Combine(TestFile.DirPath, "CommentTest_VideoCommentsEmpty.txt");
            var html = File.ReadAllText(file);
            var extractor = new CommentPageExtractor(html);
            var continuationComment = extractor.TryGetPageContinuation();
            var contextComment = extractor.TryGetInnerTubeContext();

            Assert.IsNull(continuationComment);
            Assert.IsNotNull(contextComment);
        }

        [TestMethod]
        public void CommunityCommentsReload()
        {
            var file = Path.Combine(TestFile.DirPath, "CommentTest_CommunityCommentsReload.txt");
            var json = File.ReadAllText(file);
            var extractor = new CommentPageExtractor(json);
            var commentItems = extractor.GetCommentItemsFromNext().ToList();
            var continuation = extractor.TryGetContinuation();

            var comments = MapList(commentItems);
            var third = comments.Skip(2).First();

            Assert.IsNotNull(continuation);
            Assert.AreEqual(comments.Count, 20);
            Assert.AreEqual(third.IsPinned, false);
            Assert.AreEqual(third.AuthorIsChannelOwner, false);
            Assert.AreEqual(third.ReplyCount, 17);
            Assert.AreEqual(third.LikeCount, 1200);
            Assert.AreNotEqual(third.CommentId, "");
            Assert.AreNotEqual(third.AuthorChannelId, "");
        }

        [TestMethod]
        public void CommunityCommentsAppend()
        {
            var file = Path.Combine(TestFile.DirPath, "CommentTest_CommunityCommentsAppend.txt");
            var json = File.ReadAllText(file);
            var extractor = new CommentPageExtractor(json);
            var commentItems = extractor.GetCommentItemsFromNext().ToList();
            var continuation = extractor.TryGetContinuation();

            var comments = MapList(commentItems);
            var second = comments.Skip(1).First();

            Assert.IsNotNull(continuation);
            Assert.AreEqual(comments.Count, 20);
            Assert.AreEqual(second.IsPinned, false);
            Assert.AreEqual(second.AuthorIsChannelOwner, false);
            Assert.AreEqual(second.ReplyCount, 13);
            Assert.AreEqual(second.LikeCount, 537);
            Assert.AreNotEqual(second.CommentId, "");
            Assert.AreNotEqual(second.AuthorChannelId, "");
        }

        internal List<Comment> MapList(List<JToken> commentItems)
        {
            var comments = new List<Comment>();
            foreach (var item in commentItems)
            {
                comments.Add(Map(item));
            }
            return comments;
        }

        internal Comment Map(JToken content)
        {
            var extractor = new CommentExtractor(content);
            return new Comment
            {
                CommentId = extractor.GetCommentId(),
                Content = extractor.GetContent(),
                IsModerated = extractor.IsModerated(),
                PublishedTime = extractor.GetPublishedTime(),
                PublishedTimeSeconds = extractor.GetPublishedTimeSeconds(),
                LikeCount = extractor.GetLikeCount(),
                AuthorTitle = extractor.GetAuthorTitle(),
                AuthorChannelId = extractor.GetAuthorChannelId(),
                AuthorThumbnails = extractor.GetAuthorThumbnails(),
                AuthorIsChannelOwner = extractor.GetAuthorIsChannelOwner(),
                IsPinned = extractor.IsPinned(),
                ReplyCount = extractor.GetReplyCount(),
                CommentType = extractor.GetCommentType(),
                Amount = extractor.GetAmount(),
                AmountColor = extractor.TryGetAmountColor(),
                _continuation = extractor.TryGetReplyContinuation()
            };
        }
    }
}
