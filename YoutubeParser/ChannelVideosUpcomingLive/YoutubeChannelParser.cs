using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.ChannelVideos;
using YoutubeParser.Commons;
using YoutubeParser.Extensions;
using YoutubeParser.Utils;

namespace YoutubeParser.Channels
{
    public partial class YoutubeChannelParser
    {
        // ----- GetVideos -----
        private string? _continuationUpcomingLive;
        private JToken? _contextUpcomingLive;

        public async Task<List<ChannelVideo>> GetUpcomingLiveListAsync(string urlOrChannelId)
        {
            var url = $"{GetChannelUrl(urlOrChannelId)}/videos?view=2&live_view=502";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var extractor = new ChannelVideoPageExtractor(html);

            var videos = new List<ChannelVideo>();
            var videoItems = extractor.GetVideoItems();
            foreach (var item in videoItems)
            {
                var video = MapVideo(item);
                videos.Add(video);
            }
            // must be after each GetVideoItems
            _continuationUpcomingLive = extractor.TryGetContinuation();
            _contextUpcomingLive = extractor.TryGetInnerTubeContext();
            return videos;
        }

        public async Task<List<ChannelVideo>?> GetNextUpcomingLiveListAsync()
        {
            if (_continuationUpcomingLive == null)
                return null;

            var apiUrl = $"https://www.youtube.com/youtubei/v1/browse?key={apiKey}";
            var client = _httpClient;

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            var payload = new
            {
                context = _contextUpcomingLive,
                continuation = _continuationUpcomingLive
            };
            var content = new StringContent(
                JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            request.Content = content;
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var extractor = new ChannelVideoPageExtractor(json);

            var videos = new List<ChannelVideo>();
            var videoItems = extractor.GetVideoItemsFromNext();
            foreach (var item in videoItems)
            {
                videos.Add(MapVideo(item));
            }
            // must be after each GetVideoItemsFromNext
            _continuationUpcomingLive = extractor.TryGetContinuation();
            return videos;
        }

#if (!NET45 && !NET46)
        public async IAsyncEnumerable<ChannelVideo> GetUpcomingLiveAsync(string urlOrChannelId)
        {
            var videos = await GetUpcomingLiveListAsync(urlOrChannelId);
            foreach (var item in videos)
            {
                yield return item;
            }
            while (true)
            {
                var nextVideos = await GetNextUpcomingLiveListAsync();
                if (nextVideos == null)
                    break;
                foreach (var item in nextVideos)
                {
                    yield return item;
                }
            }
        }
#endif
        // ----- GetVideos -----
    }
}
