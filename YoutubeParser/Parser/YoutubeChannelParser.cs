﻿using Newtonsoft.Json;
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

        // ----- GetChannel -----
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
            var extractor = new ChannelPageExtractor(data);
            var channel = new Channel
            {
                Title = extractor.GetTitle(),
                ChannelId = extractor.GetChannelId(),
                Description = extractor.GetDescription(),
                CanonicalChannelUrl = extractor.GetCanonicalChannelUrl(),
                Country = extractor.GetCountry(),
                SubscriberCount = extractor.GetSubscriberCount(),
                ViewCount = extractor.GetViewCount(),
                JoinedDate = extractor.GetJoinedDate(),
                Thumbnails = extractor.GetThumbnails(),
                Banners = extractor.GetBanners(),
            };
            return channel;
        }
        // ----- GetChannel -----

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
            var extractor = new ChannelVideoPageExtractor(data);
            
            _continuation = null;
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
            var data = JsonConvert.DeserializeObject<JObject>(json);
            var extractor = new ChannelVideoPageExtractor(data);
            
            _continuation = null;
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
