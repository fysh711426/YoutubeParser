using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
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
        // ----- GetTopLiveChats -----
        private string? _continuationTopLiveChat;
        private JToken? _contextTopLiveChat;
        private bool _isTopReplay = true;

        private LiveChat MapTopLiveChat(JToken content)
        {
            var liveChat = MapLiveChatBase(content);
            if (!_isTopReplay)
                liveChat.TimestampText = liveChat.TimestampUsec
                    .GetTimestampUsecText();
            return liveChat;
        }

        /// <summary>
        /// Get video top chat list by video url or id.
        /// </summary>
        /// <param name="urlOrVideoId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<LiveChat>> GetTopChatsListAsync(string urlOrVideoId, CancellationToken token = default)
        {
            _topLiveChatDict = new();
            var url = $"{GetVideoUrl(urlOrVideoId)}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            var html = await response.Content.ReadAsStringAsync();
            _isTopReplay = new VideoPageExtractor(html)
                .GetVideoStatus() == VideoStatus.Default ? true : false;
            var extractor = new LiveChatPageExtractor(html);

            token.ThrowIfCancellationRequested();
            _continuationTopLiveChat = extractor.TryGetTopChatContinuation();
            _contextTopLiveChat = extractor.TryGetInnerTubeContext();
            if (_continuationTopLiveChat == null)
                return new List<LiveChat>();

            token.ThrowIfCancellationRequested();
            var liveChats = await GetNextTopChatsListAsync();
            return liveChats ?? new List<LiveChat>();
        }

        private Dictionary<string, LiveChat> _topLiveChatDict = new();

        /// <summary>
        /// Get next page video top chat list.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<LiveChat>?> GetNextTopChatsListAsync(CancellationToken token = default)
        {
            if (_continuationTopLiveChat == null)
                return null;

            var apiUrl = $"https://www.youtube.com/youtubei/v1/live_chat/get_live_chat{(_isTopReplay ? "_replay" : "")}?key={apiKey}";
            var client = _httpClient;

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            var payload = new
            {
                context = _contextTopLiveChat,
                continuation = _continuationTopLiveChat
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
                if (_isTopReplay)
                    return extractor.GetLiveChatReplayItemsFromNext();
                return extractor.GetLiveChatItemsFromNext();
            };

            token.ThrowIfCancellationRequested();
            var liveChatItems = getItems();
            var liveChats = new List<LiveChat>();
            foreach (var item in liveChatItems)
            {
                token.ThrowIfCancellationRequested();
                var liveChat = MapTopLiveChat(item);
                if (liveChat._liveChatType != _LiveChatType.System &&
                    liveChat._liveChatType != _LiveChatType.Placeholder)
                {
                    if (_topLiveChatDict.TryGetValue(liveChat.LiveChatId, out var prev))
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
                                _topLiveChatDict[liveChat.LiveChatId] = liveChat;
                            }
                        };
                        continue;
                    }
                    liveChats.Add(liveChat);
                    _topLiveChatDict[liveChat.LiveChatId] = liveChat;
                }
            }
            _continuationTopLiveChat = extractor.TryGetContinuation();
            _timeoutMs = extractor.TryGetTimeoutMs() ?? 0;
            if (!_loop)
                if (liveChats.Count == 0)
                _continuationTopLiveChat = null;
            return liveChats;
        }

#if (!NET45 && !NET46)
        /// <summary>
        /// Merge get and next method, and add delay between request.
        /// </summary>
        /// <param name="urlOrCommunityId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<LiveChat> GetTopChatsAsync(string urlOrCommunityId,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var liveChats = await GetTopChatsListAsync(urlOrCommunityId, token);
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
                var nextLiveChats = await GetNextTopChatsListAsync(token);
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
        // ----- GetTopLiveChats -----
    }
}
