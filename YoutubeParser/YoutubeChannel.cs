using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.Extensions;
using YoutubeParser.Models;
using YoutubeParser.Utils;

namespace YoutubeParser
{
    public class YoutubeChannel
    {
        private static readonly string userAgent =
            @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36";
        private readonly HttpClient _httpClient;

        private string _url;
        public YoutubeChannel(string urlOrChannelId) : 
            this(Http.Client, urlOrChannelId) 
        { 
        }
        public YoutubeChannel(HttpClient httpClient, string urlOrChannelId)
        {
            _httpClient = httpClient;
            if (httpClient.DefaultRequestHeaders.ConnectionClose == null)
                _httpClient.DefaultRequestHeaders.ConnectionClose = true;
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
            _url = urlOrChannelId;
            if (!urlOrChannelId.Contains("www.youtube.com"))
                _url = $"https://www.youtube.com/channel/{urlOrChannelId}";
        }

        // ----- GetAbout -----
        public async Task<About> GetAboutAsync()
        {
            var url = $"{_url}/about";
            var client = _httpClient;
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var json = html
                .Pipe(it => Regex.Matches(it, @"<script.*?>([\s\S]*?)<\/script>"))
                .SelectMany(it => it.Groups[1].Value)
                .Where(it => it.Contains("ytInitialData"))
                .First()
                .Pipe(it => it
                    .Substring(0, it.Length - 1)
                    .Replace("var ytInitialData = ", ""));
            var data = JsonConvert.DeserializeObject<JObject>(json);
            var header = data?["header"]?["c4TabbedHeaderRenderer"];
            var aboutTab = data?["contents"]?["twoColumnBrowseResultsRenderer"]?["tabs"]?
                .Values<JObject>()
                .Where(it => it?["tabRenderer"]?["selected"]?.Value<bool>() == true)
                .FirstOrDefault();
            var channelAbout = aboutTab?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?[0]?["itemSectionRenderer"]?["contents"]?[0]?["channelAboutFullMetadataRenderer"]?
                .Value<JObject>();
            var about = new About
            {
                Title = header?["title"]?.Value<string>() ?? "",
                ChannelId = header?["channelId"]?.Value<string>() ?? "",
                Subscriber = header?["subscriberCountText"]?["simpleText"]?.Value<string>() ?? "",
                Thumbnails = header?["avatar"]?["thumbnails"]?
                    .Values<JObject>()
                    .Select(it => new Thumbnail
                    {
                        Url = it?["url"]?.Value<string>() ?? "",
                        Width = it?["width"]?.Value<int>() ?? 0,
                        Height = it?["height"]?.Value<int>() ?? 0
                    })
                    .ToList() ?? new List<Thumbnail>(),
                Banners = header?["banner"]?["thumbnails"]?
                    .Values<JObject>()
                    .Select(it => new Thumbnail
                    {
                        Url = it?["url"]?.Value<string>() ?? "",
                        Width = it?["width"]?.Value<int>() ?? 0,
                        Height = it?["height"]?.Value<int>() ?? 0
                    })
                    .ToList() ?? new List<Thumbnail>(),
                Description = channelAbout?["description"]?["simpleText"]?.Value<string>() ?? "",
                ViewCount = channelAbout?["viewCountText"]?["simpleText"]?.Value<string>() ?? "",
                JoinedDate = channelAbout?["joinedDateText"]?["runs"]?[1]?["text"]?.Value<string>() ?? "",
                CanonicalChannelUrl = channelAbout?["canonicalChannelUrl"]?.Value<string>() ?? "",
                Country = channelAbout?["country"]?["simpleText"]?.Value<string>() ?? ""
            };
            return about;
        }
        // ----- GetAbout -----

        // ----- GetVideos -----
        private Continuation? _continuation;
        private JToken? _context;
        private string? _visitor;
        private string? _clientVersion;

        private Video MapVideo(JToken? grid)
        {
            return new Video
            {
                VideoId = grid?["videoId"]?.Value<string>() ?? "",
                Title = grid?["title"]?["runs"]?[0]?["text"]?.Value<string>() ?? "",
                PublishedTime = grid?["publishedTimeText"]?["simpleText"]?.Value<string>() ?? "",
                ViewCount = grid?["viewCountText"]?["simpleText"]?.Value<string>() ?? "",
                Duration = grid?["thumbnailOverlays"]?[0]?["thumbnailOverlayTimeStatusRenderer"]?["text"]?["simpleText"]?.Value<string>() ?? "",
                Thumbnails = grid?["thumbnail"]?["thumbnails"]?
                    .Values<JObject>()
                    .Select(it => new Thumbnail
                    {
                        Url = it?["url"]?.Value<string>() ?? "",
                        Width = it?["width"]?.Value<int>() ?? 0,
                        Height = it?["height"]?.Value<int>() ?? 0
                    })
                    .ToList() ?? new List<Thumbnail>(),
                RichThumbnails = grid?["richThumbnail"]?["movingThumbnailRenderer"]?["movingThumbnailDetails"]?["thumbnails"]?
                    .Values<JObject>()
                    .Select(it => new Thumbnail
                    {
                        Url = it?["url"]?.Value<string>() ?? "",
                        Width = it?["width"]?.Value<int>() ?? 0,
                        Height = it?["height"]?.Value<int>() ?? 0
                    })
                    .ToList() ?? new List<Thumbnail>()
            };
        }

        public async Task<List<Video>> GetVideosListAsync()
        {
            var url = $"{_url}/videos";
            var client = _httpClient;
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var json = html
                .Pipe(it => Regex.Matches(it, @"<script.*?>([\s\S]*?)<\/script>"))
                .SelectMany(m => m.Groups[1].Value)
                .Where(it => it.Contains("ytInitialData"))
                .First()
                .Pipe(it => it
                    .Substring(0, it.Length - 1)
                    .Replace("var ytInitialData = ", ""));
            var data = JsonConvert.DeserializeObject<JObject>(json);
            var videoTab = data?["contents"]?["twoColumnBrowseResultsRenderer"]?["tabs"]?
                .Values<JObject>()
                .Where(it => it?["tabRenderer"]?["selected"]?.Value<bool>() == true)
                .FirstOrDefault();
            var videoGrids = videoTab?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?[0]?["itemSectionRenderer"]?["contents"]?[0]?["gridRenderer"]?["items"]?
                .Values<JObject>() ?? new List<JObject>();

            _continuation = null;
            var videos = new List<Video>();
            foreach (var videoGrid in videoGrids)
            {
                if (videoGrid?.ContainsKey("gridVideoRenderer") == true)
                {
                    var grid = videoGrid["gridVideoRenderer"];
                    var video = MapVideo(grid);
                    videos.Add(video);
                    continue;
                }
                _continuation = new Continuation
                {
                    ctp = videoGrid?["continuationItemRenderer"]?["continuationEndpoint"]?["clickTrackingParams"]?.Value<string>() ?? "",
                    continuation = videoGrid?["continuationItemRenderer"]?["continuationEndpoint"]?["continuationCommand"]?["token"]?.Value<string>() ?? ""
                };
            }
            var ytcfg = html
                .Pipe(it => Regex.Match(it, @"ytcfg\.set\s*\(\s*({.+?})\s*\)\s*;"))
                .Select(m => m.Groups[1].Value)
                .Pipe(it => JsonConvert.DeserializeObject<JObject>(it));
            _context = ytcfg?["INNERTUBE_CONTEXT"];
            _clientVersion = ytcfg?["INNERTUBE_CLIENT_VERSION"]?.Value<string>() ?? "";
            _visitor = _context?["client"]?["visitorData"]?.Value<string>() ?? "";
            return videos;
        }

        public async Task<List<Video>?> GetNextVideosListAsync()
        {
            if (_continuation == null)
                return null;

            var apiKey = "AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8";
            var apiUrl = $"https://www.youtube.com/youtubei/v1/browse?key={apiKey}";
            var client = _httpClient;

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            request.Headers.Add("x-youtube-client-name", "1");
            request.Headers.Add("youtube-client-version", "1");
            request.Headers.Add("x-youtube-client-name", _clientVersion);
            if (_visitor != "")
                request.Headers.Add("x-goog-visitor-id", _visitor);
            var postData = new
            {
                context = _context,
                continuation = _continuation.continuation,
                clickTracking = new
                {
                    clickTrackingParams = _continuation.ctp
                }
            };
            var content = new StringContent(
                JsonConvert.SerializeObject(postData), Encoding.UTF8, "application/json");
            request.Content = content;
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<JObject>(json);
            var videoGrids = data?["onResponseReceivedActions"]?[0]?["appendContinuationItemsAction"]?["continuationItems"]?
                .Values<JObject>() ?? new List<JObject>();

            _continuation = null;
            var videos = new List<Video>();
            foreach (var videoGrid in videoGrids)
            {
                if (videoGrid?.ContainsKey("gridVideoRenderer") == true)
                {
                    var grid = videoGrid["gridVideoRenderer"];
                    var video = MapVideo(grid);
                    videos.Add(video);
                    continue;
                }
                _continuation = new Continuation
                {
                    ctp = videoGrid?["continuationItemRenderer"]?["continuationEndpoint"]?["clickTrackingParams"]?.Value<string>() ?? "",
                    continuation = videoGrid?["continuationItemRenderer"]?["continuationEndpoint"]?["continuationCommand"]?["token"]?.Value<string>() ?? ""
                };
            }
            return videos;
        }

#if (!NET45 && !NET46)
        public async IAsyncEnumerable<Video> GetVideosAsync()
        {
            var videos = await GetVideosListAsync();
            foreach(var item in videos)
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
