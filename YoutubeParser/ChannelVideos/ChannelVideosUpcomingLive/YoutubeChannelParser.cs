using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeParser.ChannelVideos;

namespace YoutubeParser.Channels
{
    public partial class YoutubeChannelParser
    {
        // ----- GetVideos -----
        private string? _continuationUpcomingLive;
        private JToken? _contextUpcomingLive;

        /// <summary>
        /// Get channel upcoming live video list by channel url or id.
        /// </summary>
        /// <param name="urlOrChannelId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [Obsolete]
        public async Task<List<ChannelVideo>> GetUpcomingLiveListAsync(string urlOrChannelId, CancellationToken token = default)
        {
            var url = $"{GetChannelUrl(urlOrChannelId)}/videos?view=2&live_view=502";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            var html = await response.Content.ReadAsStringAsync();
            var extractor = new ChannelVideoPageExtractor(html);

            token.ThrowIfCancellationRequested();
            var subMenuTitle = extractor.GetSelectedSubMenuTitle();
            if (subMenuTitle != "Upcoming live streams")
                return new List<ChannelVideo>();

            var videos = new List<ChannelVideo>();
            var videoItems = extractor.GetVideoItems();
            foreach (var item in videoItems)
            {
                token.ThrowIfCancellationRequested();
                videos.Add(MapVideo(item));
            }
            // must be after each GetVideoItems
            _continuationUpcomingLive = extractor.TryGetContinuation();
            _contextUpcomingLive = extractor.TryGetInnerTubeContext();
            return videos;
        }

        /// <summary>
        /// Get next page channel upcoming live video list.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [Obsolete]
        public async Task<List<ChannelVideo>?> GetNextUpcomingLiveListAsync(CancellationToken token = default)
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
            using var response = await client.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            var json = await response.Content.ReadAsStringAsync();
            var extractor = new ChannelVideoPageExtractor(json);

            token.ThrowIfCancellationRequested();
            var videos = new List<ChannelVideo>();
            var videoItems = extractor.GetVideoItemsFromNext();
            foreach (var item in videoItems)
            {
                token.ThrowIfCancellationRequested();
                videos.Add(MapVideo(item));
            }
            // must be after each GetVideoItemsFromNext
            _continuationUpcomingLive = extractor.TryGetContinuation();
            return videos;
        }

#if (!NET45 && !NET46)
        /// <summary>
        /// Merge get and next method, and add delay between request.
        /// </summary>
        /// <param name="urlOrChannelId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [Obsolete]
        public async IAsyncEnumerable<ChannelVideo> GetUpcomingLiveAsync(string urlOrChannelId,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var videos = await GetUpcomingLiveListAsync(urlOrChannelId, token);
            foreach (var item in videos)
            {
                token.ThrowIfCancellationRequested();
                yield return item;
            }
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (_requestDelay != null)
                    await Task.Delay(_requestDelay(), token);
                var nextVideos = await GetNextUpcomingLiveListAsync(token);
                if (nextVideos == null)
                    break;
                foreach (var item in nextVideos)
                {
                    token.ThrowIfCancellationRequested();
                    yield return item;
                }
            }
        }
#endif
        // ----- GetVideos -----
    }
}
