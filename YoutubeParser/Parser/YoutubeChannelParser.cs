using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.Channels;
using YoutubeParser.Extensions;
using YoutubeParser.Models;
using YoutubeParser.Utils;

namespace YoutubeParser.Parser
{
    public class YoutubeChannelParser : YoutubeParserBase
    {
        public YoutubeChannelParser(HttpClient httpClient)
            : base(httpClient)
        {
        }

        // ----- GetAbout -----
        public async Task<Channel> GetAsync(string urlOrChannelId)
        {
            var url = $"{GetChannelUrl(urlOrChannelId)}/about";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", $"PREF=hl={hl}");
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
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

            var about = new Channel
            {
                Title = header?["title"]?.Value<string>() ?? "",
                ChannelId = header?["channelId"]?.Value<string>() ?? "",
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
                CanonicalChannelUrl = channelAbout?["canonicalChannelUrl"]?.Value<string>() ?? "",
                Country = channelAbout?["country"]?["simpleText"]?.Value<string>() ?? "",
                SubscriberCount = header?["subscriberCountText"]?["simpleText"]?.Value<string>()?.GetCountValue() ?? 0,
                ViewCount = channelAbout?["viewCountText"]?["simpleText"]?.Value<string>()?.GetCountValue() ?? 0,
                JoinedDate = channelAbout?["joinedDateText"]?["runs"]?[1]?["text"]?.Value<string>()?.GetJoinedDate()
            };
            return about;
        }
        // ----- GetAbout -----

        // ----- GetVideos -----
        private string? _continuation;
        private JToken? _context;

        private ChannelVideo MapVideo(JToken? grid)
        {
            var duration = grid?["thumbnailOverlays"]?[0]?["thumbnailOverlayTimeStatusRenderer"]?["text"]?["simpleText"]?.Value<string>();
            var publishedTime = grid?["publishedTimeText"]?["simpleText"]?.Value<string>();
            var viewCount = grid?["viewCountText"]?["simpleText"]?.Value<string>()?.GetCountValue() ?? 0;
            var liveViewCount = grid?["viewCountText"]?["runs"]?.FirstOrDefault()?["text"]?.Value<string>()?.GetCountValue() ?? 0;
            var timeStyle = grid?["thumbnailOverlays"]?[0]?["thumbnailOverlayTimeStatusRenderer"]?["style"]?.Value<string>();
            var isStream = publishedTime?.Contains("Streamed") ?? false;
            var isLive = timeStyle == "LIVE";
            var isShorts = duration == "SHORTS";

            return new ChannelVideo
            {
                VideoId = grid?["videoId"]?.Value<string>() ?? "",
                Title = grid?["title"]?["runs"]?[0]?["text"]?.Value<string>() ?? "",
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
                    .ToList() ?? new List<Thumbnail>(),
                PublishedTime = publishedTime ?? "",
                PublishedTimeSeconds = publishedTime?.GetPublishedTimeSeconds() ?? 0,
                IsShorts = isShorts,
                Duration = !isShorts ? duration?.GetDuration() : null,
                IsLive = isLive,
                IsStream = isLive || isStream,
                ViewCount = isLive ? liveViewCount : viewCount
            };
        }

        public async Task<List<ChannelVideo>> GetVideosListAsync(string urlOrChannelId)
        {
            var url = $"{GetChannelUrl(urlOrChannelId)}/videos";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", $"PREF=hl={hl}");
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
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
            var videos = new List<ChannelVideo>();
            foreach (var videoGrid in videoGrids)
            {
                if (videoGrid?.ContainsKey("gridVideoRenderer") == true)
                {
                    var grid = videoGrid["gridVideoRenderer"];
                    var video = MapVideo(grid);
                    videos.Add(video);
                    continue;
                }
                _continuation = videoGrid?["continuationItemRenderer"]?["continuationEndpoint"]?["continuationCommand"]?["token"]?.Value<string>();
            }
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
            var data = JsonConvert.DeserializeObject<JObject>(json);
            var videoGrids = data?["onResponseReceivedActions"]?[0]?["appendContinuationItemsAction"]?["continuationItems"]?
                .Values<JObject>() ?? new List<JObject>();

            _continuation = null;
            var videos = new List<ChannelVideo>();
            foreach (var videoGrid in videoGrids)
            {
                if (videoGrid?.ContainsKey("gridVideoRenderer") == true)
                {
                    var grid = videoGrid["gridVideoRenderer"];
                    var video = MapVideo(grid);
                    videos.Add(video);
                    continue;
                }
                _continuation = videoGrid?["continuationItemRenderer"]?["continuationEndpoint"]?["continuationCommand"]?["token"]?.Value<string>();
            }
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
