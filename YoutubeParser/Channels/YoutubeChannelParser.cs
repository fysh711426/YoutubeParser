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

namespace YoutubeParser.Channels
{
    public partial class YoutubeChannelParser : YoutubeParserBase
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
    }
}
