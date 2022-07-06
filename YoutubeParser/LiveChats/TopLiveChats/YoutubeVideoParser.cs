﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YoutubeParser.Comments;
using YoutubeParser.LiveChats;
using YoutubeParser.Shares;

namespace YoutubeParser.Videos
{
    public partial class YoutubeVideoParser
    {
        // ----- GetTopLiveChats -----
        private string? _continuationTopLiveChat;
        private JToken? _contextTopLiveChat;

        public async Task<List<LiveChat>> GetTopChatsListAsync(string urlOrVideoId)
        {
            _topLiveChatDict = new();
            var url = $"{GetVideoUrl(urlOrVideoId)}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var extractor = new LiveChatPageExtractor(html);
            _continuationTopLiveChat = extractor.TryGetTopChatContinuation();
            _contextTopLiveChat = extractor.TryGetInnerTubeContext();
            if (_continuationTopLiveChat == null)
                return new List<LiveChat>();
            var liveChats = await GetNextTopChatsListAsync();
            return liveChats ?? new List<LiveChat>();
        }

        private Dictionary<string, bool> _topLiveChatDict = new();
        public async Task<List<LiveChat>?> GetNextTopChatsListAsync()
        {
            if (_continuationTopLiveChat == null)
                return null;

            var apiUrl = $"https://www.youtube.com/youtubei/v1/live_chat/get_live_chat_replay?key={apiKey}";
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
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var extractor = new LiveChatPageExtractor(json);

            var liveChats = new List<LiveChat>();
            var liveChatItems = extractor.GetLiveChatItemsFromNext();
            foreach (var item in liveChatItems)
            {
                var liveChat = MapLiveChat(item);
                if (liveChat.LiveChatType != LiveChatType.System &&
                    liveChat.LiveChatType != LiveChatType.Placeholder)
                {
                    if (!_topLiveChatDict.ContainsKey(liveChat.LiveChatId))
                        liveChats.Add(MapLiveChat(item));
                    _topLiveChatDict[liveChat.LiveChatId] = true;
                }
            }
            _continuationTopLiveChat = extractor.TryGetContinuation();
            return liveChats;
        }

#if (!NET45 && !NET46)
        public async IAsyncEnumerable<LiveChat> GetTopChatsAsync(string urlOrCommunityId)
        {
            var liveChats = await GetTopChatsListAsync(urlOrCommunityId);
            foreach (var item in liveChats)
            {
                yield return item;
            }
            while (true)
            {
                var nextLiveChats = await GetNextTopChatsListAsync();
                if (nextLiveChats == null)
                    break;
                foreach (var item in nextLiveChats)
                {
                    yield return item;
                }
            }
        }
#endif
        // ----- GetTopLiveChats -----
    }
}