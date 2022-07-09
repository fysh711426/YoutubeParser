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
            var file = Path.Combine(TestFile.DirPath, "LiveChat_SuperChat_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new LiveChatPageExtractor(html);
            var liveChatItems = extractor.GetLiveChatReplayItemsFromNext().ToList();
            var continuation = extractor.TryGetContinuation();

            var liveChats = MapList(liveChatItems);
            var superChats = liveChats
                .Where(it => it.LiveChatType == LiveChatType.SuperChat)
                .ToList();

            var first = superChats.First();
            var darkBlue = superChats.Skip(11).First();
            var lightBlue = superChats.Skip(10).First();
            var green = superChats.Skip(8).First();
            var orange = superChats.Skip(3).First();
            var purple = superChats.Skip(0).First();
            var red = superChats.Skip(2).First();

            Assert.IsNotNull(continuation);
            Assert.AreEqual(superChats.Count, 18);
            Assert.AreNotEqual(first.LiveChatId, "");
            Assert.AreNotEqual(first.AuthorChannelId, "");
            Assert.AreNotEqual(first.TimestampText, "");
            Assert.AreEqual(darkBlue.AmountColor, AmountColor.DarkBlue);
            Assert.AreEqual(lightBlue.AmountColor, AmountColor.LightBlue);
            Assert.AreEqual(green.AmountColor, AmountColor.Green);
            Assert.AreEqual(orange.AmountColor, AmountColor.Orange);
            Assert.AreEqual(purple.AmountColor, AmountColor.Purple);
            Assert.AreEqual(red.AmountColor, AmountColor.Red);
        }

        [TestMethod]
        public void GiftChat()
        {
            var file = Path.Combine(TestFile.DirPath, "LiveChat_GiftChat_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new LiveChatPageExtractor(html);
            var liveChatItems = extractor.GetLiveChatReplayItemsFromNext().ToList();
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
            var file = Path.Combine(TestFile.DirPath, "LiveChat_MembershipChat_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new LiveChatPageExtractor(html);
            var liveChatItems = extractor.GetLiveChatReplayItemsFromNext().ToList();
            var continuation = extractor.TryGetContinuation();

            var liveChats = MapList(liveChatItems);
            var memberChats = liveChats
                .Where(it => it.LiveChatType == LiveChatType.Membership)
                .ToList();

            var first = memberChats.First();
            var other = memberChats.Skip(4).First();

            Assert.IsNotNull(continuation);
            Assert.AreEqual(memberChats.Count, 32);
            Assert.AreNotEqual(first.LiveChatId, "");
            Assert.AreNotEqual(first.AuthorChannelId, "");
            Assert.AreNotEqual(first.TimestampText, "");
            Assert.IsTrue(first.HeaderText != "");
            Assert.IsTrue(other.HeaderSubText.Contains("Welcome to"));
        }

        [TestMethod]
        public void PinnedChat()
        {
            var file = Path.Combine(TestFile.DirPath, "LiveChat_PinnedChat_Html.txt");
            var html = File.ReadAllText(file);
            var extractor = new LiveChatPageExtractor(html);
            var liveChatItems = extractor.GetLiveChatReplayItemsFromNext().ToList();
            var continuation = extractor.TryGetContinuation();

            var liveChats = MapList(liveChatItems);
            var pinnedChats = liveChats
                .Where(it => it.IsPinned)
                .ToList();

            var first = pinnedChats.First();

            Assert.IsNotNull(continuation);
            Assert.AreEqual(pinnedChats.Count, 1);
            Assert.AreNotEqual(first.LiveChatId, "");
            Assert.AreNotEqual(first.AuthorChannelId, "");
            Assert.AreNotEqual(first.TimestampText, "");
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
                    throw new Exception("LiveChatType unknow.");
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
