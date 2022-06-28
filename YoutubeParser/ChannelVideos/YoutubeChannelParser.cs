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
                VideoStatus = extractor.GetVideoStatus(),
                VideoType = extractor.GetVideoType(),
                IsShorts = extractor.IsShorts()
            };
        }

        public async Task<List<ChannelVideo>> GetVideosListAsync(string urlOrChannelId)
        {
            var url = $"{GetChannelUrl(urlOrChannelId)}/videos";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var extractor = html
                .Pipe(it => new YoutubePageExtractor(it))
                .Pipe(it => new ChannelVideoPageExtractor(it.TryGetInitialData()));
            
            var videos = new List<ChannelVideo>();
            var videoItems = extractor.GetVideoItems();
            foreach (var item in videoItems)
            {
                videos.Add(MapVideo(item));
            }
            // must be after each GetVideoItems
            _continuation = extractor.TryGetContinuation();
            var ytcfg = html
                .Pipe(it => Regex.Match(it, @"ytcfg\.set\s*\(\s*({.+?})\s*\)\s*;"))
                .Select(m => m.Groups[1].Value)
                .Pipe(it => JsonConvert.DeserializeObject<JObject>(it));
            _context = ytcfg?["INNERTUBE_CONTEXT"];
            return videos;
        }

        public async Task<List<ChannelVideo>?> GetNextVideosListAsync()
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
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var extractor = json
                .Pipe(it => JsonConvert.DeserializeObject<JObject>(it))
                .Pipe(it => new ChannelVideoPageExtractor(it));

            var videos = new List<ChannelVideo>();
            var videoItems = extractor.GetVideoItemsFromNext();
            foreach (var item in videoItems)
            {
                videos.Add(MapVideo(item));
            }
            // must be after each GetVideoItemsFromNext
            _continuation = extractor.TryGetContinuation();
            return videos;
        }

#if (!NET45 && !NET46)
        public async IAsyncEnumerable<ChannelVideo> GetVideosAsync(string urlOrChannelId)
        {
            var videos = await GetVideosListAsync(urlOrChannelId);
            foreach (var item in videos)
            {
                yield return item;
            }
            while (true)
            {
                var nextVideos = await GetNextVideosListAsync();
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
