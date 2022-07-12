using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeParser.Shares;

namespace YoutubeParser.Channels
{
    public partial class YoutubeChannelParser : YoutubeParserBase
    {
        public YoutubeChannelParser(
            HttpClient httpClient, Func<int>? requestDelay)
                : base(httpClient, requestDelay)
        {
        }

        // ----- GetChannel -----

        /// <summary>
        /// Get channel info by channel url or id.
        /// </summary>
        /// <param name="urlOrChannelId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Channel> GetAsync(string urlOrChannelId, CancellationToken token = default)
        {
            var url = $"{GetChannelUrl(urlOrChannelId)}/about";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            var html = await response.Content.ReadAsStringAsync();
            var extractor = new ChannelPageExtractor(html);
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
