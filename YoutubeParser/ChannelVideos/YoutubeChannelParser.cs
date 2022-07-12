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
        // ----- GetVideos -----
        private string? _continuation;
        private JToken? _context;

        private ChannelVideo MapVideo(JToken grid)
        {
            var extractor = new ChannelVideoExtractor(grid);
            return new ChannelVideo
            {
                Title = extractor.GetTitle(),
                VideoId = extractor.GetVideoId(),
                Thumbnails = extractor.GetThumbnails(),
                RichThumbnail = extractor.TryGetRichThumbnail(),
                ViewCount = extractor.GetViewCount(),
                Duration = extractor.TryGetDuration(),
                PublishedTime = extractor.GetPublishedTime(),
                PublishedTimeSeconds = extractor.GetPublishedTimeSeconds(),
                UpcomingDate = extractor.TryGetUpcomingDate(),
                VideoStatus = extractor.GetVideoStatus(),
                VideoType = extractor.GetVideoType(),
                IsShorts = extractor.IsShorts()
            };
        }

        /// <summary>
        /// Get channel video list by channel url or id.
        /// </summary>
        /// <param name="urlOrChannelId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<ChannelVideo>> GetVideosListAsync(string urlOrChannelId, CancellationToken token = default)
        {
            var url = $"{GetChannelUrl(urlOrChannelId)}/videos";
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
            _continuation = extractor.TryGetContinuation();
            _context = extractor.TryGetInnerTubeContext();
            return videos;
        }

        /// <summary>
        /// Get next page channel video list.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<ChannelVideo>?> GetNextVideosListAsync(CancellationToken token = default)
        {
            if (_continuation == null)
                return null;

            var apiUrl = $"https://www.youtube.com/youtubei/v1/browse?key={apiKey}";
            var client = _httpClient;

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            var payload = new
            {
                context = _context,
                continuation = _continuation
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
            _continuation = extractor.TryGetContinuation();
            return videos;
        }

#if (!NET45 && !NET46)
        /// <summary>
        /// Merge get and next method, and add delay between request.
        /// </summary>
        /// <param name="urlOrChannelId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<ChannelVideo> GetVideosAsync(string urlOrChannelId,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var videos = await GetVideosListAsync(urlOrChannelId, token);
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
                var nextVideos = await GetNextVideosListAsync(token);
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
