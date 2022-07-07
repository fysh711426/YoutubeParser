using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using YoutubeParser.Shares;
using YoutubeParser.Utils;

namespace YoutubeParser.LiveChats
{
    internal class LiveChatPageExtractor
    {
        private readonly string? _html;

        public LiveChatPageExtractor(string? html) => _html = html;

        public JObject? TryGetInitialData() => Memo.Cache(this, () =>
            new YoutubePageExtractor(_html).TryGetInitialData()
        );

        private JToken? TryGetContent() => Memo.Cache(this, () =>
            TryGetInitialData()?["contents"]?["twoColumnWatchNextResults"]?["conversationBar"]?["liveChatRenderer"]
        );

        private JToken? TryGetTopChat() => Memo.Cache(this, () =>
            TryGetContent()?["header"]?["liveChatHeaderRenderer"]?["viewSelector"]?["sortFilterSubMenuRenderer"]?["subMenuItems"]?.Values<JObject>()?
                .FirstOrDefault(it => it?["title"]?.Value<string>()?.Contains("Top chat") == true)
        );

        private JToken? TryGetLiveChat() => Memo.Cache(this, () =>
            TryGetContent()?["header"]?["liveChatHeaderRenderer"]?["viewSelector"]?["sortFilterSubMenuRenderer"]?["subMenuItems"]?.Values<JObject>()?
                .FirstOrDefault(it => it?["title"]?.Value<string>()?.Contains("Live chat") == true)
        );

        public string? TryGetTopChatContinuation() => Memo.Cache(this, () =>
            TryGetTopChat()?["continuation"]?["reloadContinuationData"]?["continuation"]?.Value<string>()
        );

        public string? TryGetLiveChatContinuation() => Memo.Cache(this, () =>
            TryGetLiveChat()?["continuation"]?["reloadContinuationData"]?["continuation"]?.Value<string>()
        );

        public JToken? TryGetInnerTubeContext() =>
            new YoutubePageExtractor(_html)
                .TryGetYtcfg()?["INNERTUBE_CONTEXT"];

        public string? TryGetContinuation() => Memo.Cache(this, () =>
            TryGetResponseLiveChat()?["continuations"]?
                .FirstOrDefault()?["liveChatReplayContinuationData"]?["continuation"]?.Value<string>()
        );

        private JToken? TryGetResponseLiveChat() => Memo.Cache(this, () =>
            new YoutubePageExtractor(_html)
                .TryGetJsonResponse()?["continuationContents"]?["liveChatContinuation"]
        );

        // For GetNextLiveChatsList
        private IEnumerable<JObject?> GetLiveChatContentsFromNext() => Memo.Cache(this, () =>
            TryGetResponseLiveChat()?["actions"]?.Values<JObject>() ?? new List<JObject>()
        );

        public IEnumerable<JToken> GetLiveChatItemsFromNext()
        {
            var contents = GetLiveChatContentsFromNext();
            foreach (var content in contents)
            {
                var liveChat = content?["replayChatItemAction"];
                if (liveChat != null)
                {
                    yield return liveChat;
                }
            }
        }
    }
}
