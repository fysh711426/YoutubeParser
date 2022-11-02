using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        // ----- GetShorts -----
        private string? _continuationShort;
        private JToken? _contextShort;

        /// <summary>
        /// Get channel short video list by channel url or id.
        /// </summary>
        /// <param name="urlOrChannelId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<ChannelVideo>> GetShortsListAsync(string urlOrChannelId, CancellationToken token = default)
        {
            var url = $"{GetChannelUrl(urlOrChannelId)}/shorts";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            var html = await response.Content.ReadAsStringAsync();
            var extractor = new ChannelVideoPageExtractor(html);

            token.ThrowIfCancellationRequested();
            var videos = new List<ChannelVideo>();
            var videoItems = extractor.GetVideoItems();
            foreach (var item in videoItems)
            {
                token.ThrowIfCancellationRequested();
                videos.Add(MapVideo(item));
            }
            // must be after each GetVideoItems
            _continuationShort = extractor.TryGetContinuation();
            _contextShort = extractor.TryGetInnerTubeContext();
            return videos;
        }

        /// <summary>
        /// Get next page channel short video list.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<ChannelVideo>?> GetNextShortsListAsync(CancellationToken token = default)
        {
            if (_continuationShort == null)
                return null;

            var apiUrl = $"https://www.youtube.com/youtubei/v1/browse?key={apiKey}";
            var client = _httpClient;

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            var payload = new
            {
                context = _contextShort,
                continuation = _continuationShort
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
            _continuationShort = extractor.TryGetContinuation();
            return videos;
        }

#if (!NET45 && !NET46)
        /// <summary>
        /// Merge get and next method, and add delay between request.
        /// </summary>
        /// <param name="urlOrChannelId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<ChannelVideo> GetShortsAsync(string urlOrChannelId,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var videos = await GetShortsListAsync(urlOrChannelId, token);
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
                var nextVideos = await GetNextShortsListAsync(token);
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
        // ----- GetShorts -----
    }
}
