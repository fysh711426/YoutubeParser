using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeParser.Extensions;
using YoutubeParser.LiveChats;
using YoutubeParser.Shares;

namespace YoutubeParser.Videos
{
    public partial class YoutubeVideoParser
    {
        // ----- GetLiveChats -----
        private string? _continuationLiveChat;
        private JToken? _contextLiveChat;
        private bool _isReplay = true;
        private int _timeoutMs = 0;
        private bool _loop = false;

        private LiveChat MapLiveChatBase(JToken content)
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
                HeaderText = extractor.GetHeaderText(),
                HeaderSubText = extractor.GetHeaderSubText(),
                Amount = extractor.GetAmount(),
                AmountColor = extractor.TryGetAmountColor(),
                IsPinned = extractor.IsPinned(),
                TimestampUsec = extractor.GetTimestampUsec(),
                TimestampText = extractor.GetTimestampText(),
                //Json = extractor.GetJson()
            };
        }

        private LiveChat MapLiveChat(JToken content)
        {
            var liveChat = MapLiveChatBase(content);
            if (!_isReplay)
                liveChat.TimestampText = liveChat.TimestampUsec
                    .GetTimestampUsecText();
            return liveChat;
        }

        public async Task<List<LiveChat>> GetLiveChatsListAsync(string urlOrVideoId, CancellationToken token = default)
        {
            _liveChatDict = new();
            var url = $"{GetVideoUrl(urlOrVideoId)}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            var html = await response.Content.ReadAsStringAsync();
            _isReplay = new VideoPageExtractor(html)
                .GetVideoStatus() == VideoStatus.Default ? true : false;
            var extractor = new LiveChatPageExtractor(html);

            token.ThrowIfCancellationRequested();
            _continuationLiveChat = extractor.TryGetLiveChatContinuation();
            _contextLiveChat = extractor.TryGetInnerTubeContext();
            if (_continuationLiveChat == null)
                return new List<LiveChat>();

            token.ThrowIfCancellationRequested();
            var liveChats = await GetNextLiveChatsListAsync(token);
            return liveChats ?? new List<LiveChat>();
        }

        private Dictionary<string, LiveChat> _liveChatDict = new();
        public async Task<List<LiveChat>?> GetNextLiveChatsListAsync(CancellationToken token = default)
        {
            if (_continuationLiveChat == null)
                return null;

            var apiUrl = $"https://www.youtube.com/youtubei/v1/live_chat/get_live_chat{(_isReplay ? "_replay" : "")}?key={apiKey}";
            var client = _httpClient;

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            var payload = new
            {
                context = _contextLiveChat,
                continuation = _continuationLiveChat
            };
            var content = new StringContent(
                JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            request.Content = content;
            using var response = await client.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            var json = await response.Content.ReadAsStringAsync();
            var extractor = new LiveChatPageExtractor(json);

            var getItems = () =>
            {
                if (_isReplay)
                    return extractor.GetLiveChatReplayItemsFromNext();
                return extractor.GetLiveChatItemsFromNext();
            };

            token.ThrowIfCancellationRequested();
            var liveChatItems = getItems();
            var liveChats = new List<LiveChat>();
            foreach (var item in liveChatItems)
            {
                token.ThrowIfCancellationRequested();
                var liveChat = MapLiveChat(item);
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
                                // liveChat pinned may be repeat
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
            _continuationLiveChat = extractor.TryGetContinuation();
            _timeoutMs = extractor.TryGetTimeoutMs() ?? 0;
            if (!_loop)
                if (liveChats.Count == 0)
                    _continuationLiveChat = null;
            return liveChats;
        }

#if (!NET45 && !NET46)
        public async IAsyncEnumerable<LiveChat> GetLiveChatsAsync(string urlOrCommunityId,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var liveChats = await GetLiveChatsListAsync(urlOrCommunityId, token);
            foreach (var item in liveChats)
            {
                token.ThrowIfCancellationRequested();
                yield return item;
            }
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (_requestDelay != null)
                    await Task.Delay(_requestDelay(), token);
                var nextLiveChats = await GetNextLiveChatsListAsync(token);
                if (nextLiveChats == null)
                    break;
                foreach (var item in nextLiveChats)
                {
                    token.ThrowIfCancellationRequested();
                    yield return item;
                }
            }
        }
#endif
        // ----- GetLiveChats -----
    }
}
