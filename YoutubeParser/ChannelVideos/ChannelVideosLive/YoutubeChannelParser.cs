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
        private string? _continuationLive;
        private JToken? _contextLive;

        public async Task<List<ChannelVideo>> GetLiveListAsync(string urlOrChannelId)
        {
            var url = $"{GetChannelUrl(urlOrChannelId)}/videos?view=2&live_view=501";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var extractor = new ChannelVideoPageExtractor(html);

            var subMenuTitle = extractor.GetSelectedSubMenuTitle();
            if (subMenuTitle != "Live now")
                return new List<ChannelVideo>();

            var videos = new List<ChannelVideo>();
            var videoItems = extractor.GetVideoItems();
            foreach (var item in videoItems)
            {
                var video = MapVideo(item);
                videos.Add(video);
            }
            // must be after each GetVideoItems
            _continuationLive = extractor.TryGetContinuation();
            _contextLive = extractor.TryGetInnerTubeContext();
            return videos;
        }

        public async Task<List<ChannelVideo>?> GetNextLiveListAsync()
        {
            if (_continuationLive == null)
                return null;

            var apiUrl = $"https://www.youtube.com/youtubei/v1/browse?key={apiKey}";
            var client = _httpClient;

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            var payload = new
            {
                context = _contextLive,
                continuation = _continuationLive
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
            _continuationLive = extractor.TryGetContinuation();
            return videos;
        }

#if (!NET45 && !NET46)
        public async IAsyncEnumerable<ChannelVideo> GetLiveAsync(string urlOrChannelId)
        {
            var videos = await GetLiveListAsync(urlOrChannelId);
            foreach (var item in videos)
            {
                yield return item;
            }
            while (true)
            {
                var nextVideos = await GetNextLiveListAsync();
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
