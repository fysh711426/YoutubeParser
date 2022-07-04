using System.Net.Http;
using System.Threading.Tasks;
using YoutubeParser.Shares;

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
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
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
