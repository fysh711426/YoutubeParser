using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeParser.Comments;
using YoutubeParser.LiveChats;
using YoutubeParser.Shares;
using YoutubeParser.Test.Utils;

namespace YoutubeParser.Test
{
    [TestClass]
    public class LiveChatTest
    {
        [TestMethod]
        public void SuperChat()
        {
            var file = Path.Combine(TestFile.DirPath, "LiveChat_SuperChat.txt");
            var html = File.ReadAllText(file);
            var extractor = new LiveChatPageExtractor(html);
            var liveChatItems = extractor.GetLiveChatItemsFromNext().ToList();
            var continuation = extractor.TryGetContinuation();

            var liveChats = MapList(liveChatItems);
            var superChats = liveChats
                .Where(it => it.LiveChatType == LiveChatType.SuperChat)
                .ToList();

            Assert.IsNotNull(continuation);
            Assert.AreEqual(superChats.Count, 18);
            Assert.AreNotEqual(superChats[0].LiveChatId, "");
            Assert.AreNotEqual(superChats[0].AuthorChannelId, "");
            Assert.AreNotEqual(superChats[0].TimestampText, "");
            Assert.AreEqual(superChats[11].AmountColor, AmountColor.DarkBlue);
            Assert.AreEqual(superChats[10].AmountColor, AmountColor.Blue);
            Assert.AreEqual(superChats[8].AmountColor, AmountColor.Green);
            Assert.AreEqual(superChats[3].AmountColor, AmountColor.Orange);
            Assert.AreEqual(superChats[0].AmountColor, AmountColor.Purple);
            Assert.AreEqual(superChats[2].AmountColor, AmountColor.Red);
        }

        [TestMethod]
        public void GiftChat()
        {
            var file = Path.Combine(TestFile.DirPath, "LiveChat_GiftChat.txt");
            var html = File.ReadAllText(file);
            var extractor = new LiveChatPageExtractor(html);
            var liveChatItems = extractor.GetLiveChatItemsFromNext().ToList();
            var continuation = extractor.TryGetContinuation();

            var liveChats = MapList(liveChatItems);
            var giftChat = liveChats
                .FirstOrDefault(it => it.LiveChatType == LiveChatType.Gift);

            var index = 0;
            if (giftChat != null)
                index = liveChats.IndexOf(giftChat);
            var nextChat = liveChats[index + 1];

            Assert.IsNotNull(continuation);
            Assert.IsNotNull(giftChat);
            Assert.AreNotEqual(giftChat.LiveChatId, "");
            Assert.AreNotEqual(giftChat.AuthorChannelId, "");
            Assert.AreNotEqual(giftChat.TimestampUsec, 0);
            Assert.IsTrue(nextChat.Message.Contains("was gifted a membership"));
            Assert.AreNotEqual(nextChat.LiveChatId, "");
            Assert.AreNotEqual(nextChat.AuthorChannelId, "");
            Assert.AreNotEqual(nextChat.TimestampUsec, 0);
        }

        [TestMethod]
        public void MembershipChat()
        {
            var file = Path.Combine(TestFile.DirPath, "LiveChat_MembershipChat.txt");
            var html = File.ReadAllText(file);
            var extractor = new LiveChatPageExtractor(html);
            var liveChatItems = extractor.GetLiveChatItemsFromNext().ToList();
            var continuation = extractor.TryGetContinuation();

            var liveChats = MapList(liveChatItems);
            var memberChats = liveChats
                .Where(it => it.LiveChatType == LiveChatType.Membership)
                .ToList();

            Assert.IsNotNull(continuation);
            Assert.AreEqual(memberChats.Count, 32);
            Assert.AreNotEqual(memberChats[0].LiveChatId, "");
            Assert.AreNotEqual(memberChats[0].AuthorChannelId, "");
            Assert.AreNotEqual(memberChats[0].TimestampText, "");
            Assert.IsTrue(memberChats[0].HeaderText != "");
            Assert.IsTrue(memberChats[4].HeaderSubText.Contains("Welcome to"));
        }

        [TestMethod]
        public void PinnedChat()
        {
            var file = Path.Combine(TestFile.DirPath, "LiveChat_PinnedChat.txt");
            var html = File.ReadAllText(file);
            var extractor = new LiveChatPageExtractor(html);
            var liveChatItems = extractor.GetLiveChatItemsFromNext().ToList();
            var continuation = extractor.TryGetContinuation();

            var liveChats = MapList(liveChatItems);
            var pinnedChats = liveChats
                .Where(it => it.IsPinned)
                .ToList();

            Assert.IsNotNull(continuation);
            Assert.AreEqual(pinnedChats.Count, 1);
            Assert.AreNotEqual(pinnedChats[0].LiveChatId, "");
            Assert.AreNotEqual(pinnedChats[0].AuthorChannelId, "");
            Assert.AreNotEqual(pinnedChats[0].TimestampText, "");
        }

        internal List<LiveChat> MapList(List<JToken> liveChatItems)
        {
            var liveChats = new List<LiveChat>();
            var _liveChatDict = new Dictionary<string, LiveChat>();
            foreach (var item in liveChatItems)
            {
                var liveChat = Map(item);
                if (liveChat._liveChatType == _LiveChatType.Unknow)
                {
                    var x = 0;
                }
                if (liveChat._liveChatType != _LiveChatType.System &&
                    liveChat._liveChatType != _LiveChatType.Placeholder)
                {
                    if (_liveChatDict.TryGetValue(liveChat.LiveChatId, out var prev))
                    {
                        if (!prev.IsPinned && liveChat.IsPinned)
                        {
                            if (liveChats.IndexOf(prev) > -1)
                            {
                                prev.IsPinned = true;
                            }
                            else
                            {
                                liveChats.Add(liveChat);
                                _liveChatDict[liveChat.LiveChatId] = liveChat;
                            }
                        };
                        continue;
                    }
                    liveChats.Add(liveChat);
                    _liveChatDict[liveChat.LiveChatId] = liveChat;
                }
            }
            return liveChats;
        }

        internal LiveChat Map(JToken content)
        {
            var extractor = new LiveChatExtractor(content);
            return new LiveChat
            {
                _liveChatType = extractor.GetLiveChatType(),
                LiveChatType = (LiveChatType)extractor.GetLiveChatType(),
                LiveChatId = extractor.GetLiveChatId(),
                Message = extractor.GetMessage(),
                AuthorTitle = extractor.GetAuthorTitle(),
                AuthorChannelId = extractor.GetAuthorChannelId(),
                AuthorThumbnails = extractor.GetAuthorThumbnails(),
                TimestampText = extractor.GetTimestampText(),
                TimestampUsec = extractor.GetTimestampUsec(),
                HeaderText = extractor.GetHeaderText(),
                HeaderSubText = extractor.GetHeaderSubText(),
                Amount = extractor.GetAmount(),
                AmountColor = extractor.TryGetAmountColor(),
                IsPinned = extractor.IsPinned()
            };
        }
    }
}
