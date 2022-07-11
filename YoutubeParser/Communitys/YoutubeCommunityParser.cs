using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeParser.Shares;

namespace YoutubeParser.Communitys
{
    public partial class YoutubeCommunityParser : YoutubeParserBase
    {
        public YoutubeCommunityParser(
            HttpClient httpClient, Func<int>? requestDelay)
                : base(httpClient, requestDelay)
        {
        }

        // ----- GetCommunity -----
        public async Task<Community> GetAsync(string urlOrCommunityId, CancellationToken token = default)
        {
            var url = $"{GetCommunityUrl(urlOrCommunityId)}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            var html = await response.Content.ReadAsStringAsync();
            var pageExtractor = new CommunityPageExtractor(html);

            token.ThrowIfCancellationRequested();
            var item = pageExtractor.GetCommunityItems().FirstOrDefault();
            if (item == null)
                throw new Exception("Not found community.");
            var extractor = new CommunityExtractor(item);
            var community = new Community
            {
                PostId = extractor.GetPostId(),
                AuthorTitle = extractor.GetAuthorTitle(),
                AuthorChannelId = extractor.GetAuthorChannelId(),
                AuthorThumbnails = extractor.GetAuthorThumbnails(),
                Content = extractor.GetContent(),
                Images = extractor.GetImages(),
                PublishedTime = extractor.GetPublishedTime(),
                PublishedTimeSeconds = extractor.GetPublishedTimeSeconds(),
                LikeCount = extractor.GetLikeCount(),
                VoteStatus = extractor.GetVoteStatus()
            };
            return community;
        }
        // ----- GetCommunity -----
    }
}
