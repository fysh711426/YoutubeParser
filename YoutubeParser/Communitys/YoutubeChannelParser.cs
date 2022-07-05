using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YoutubeParser.Communitys;

namespace YoutubeParser.Channels
{
    public partial class YoutubeChannelParser
    {
        // ----- GetCommunitys -----
        private string? _continuationCommunity;
        private JToken? _contextCommunity;

        private Community MapCommunity(JToken content)
        {
            var extractor = new CommunityExtractor(content);
            return new Community
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
                VoteStatus = extractor.GetVoteStatus(),
                ReplyCount = extractor.GetReplyCount()
            };
        }

        public async Task<List<Community>> GetCommunitysListAsync(string urlOrChannelId)
        {
            var url = $"{GetChannelUrl(urlOrChannelId)}/community";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var extractor = new CommunityPageExtractor(html);

            var communitys = new List<Community>();
            var communityItems = extractor.GetCommunityItems();
            foreach (var item in communityItems)
            {
                communitys.Add(MapCommunity(item));
            }
            // must be after each GetVideoItems
            _continuationCommunity = extractor.TryGetContinuation();
            _contextCommunity = extractor.TryGetInnerTubeContext();
            return communitys;
        }

        public async Task<List<Community>?> GetNextCommunitysListAsync()
        {
            if (_continuationCommunity == null)
                return null;

            var apiUrl = $"https://www.youtube.com/youtubei/v1/browse?key={apiKey}";
            var client = _httpClient;

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            var payload = new
            {
                context = _contextCommunity,
                continuation = _continuationCommunity
            };
            var content = new StringContent(
                JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            request.Content = content;
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var extractor = new CommunityPageExtractor(json);

            var communitys = new List<Community>();
            var communityItems = extractor.GetCommunityItemsFromNext();
            foreach (var item in communityItems)
            {
                communitys.Add(MapCommunity(item));
            }
            // must be after each GetCommunityItemsFromNext
            _continuationCommunity = extractor.TryGetContinuation();
            return communitys;
        }

#if (!NET45 && !NET46)
        public async IAsyncEnumerable<Community> GetCommunitysAsync(string urlOrChannelId)
        {
            var communitys = await GetCommunitysListAsync(urlOrChannelId);
            foreach (var item in communitys)
            {
                yield return item;
            }
            while (true)
            {
                var nextCommunitys = await GetNextCommunitysListAsync();
                if (nextCommunitys == null)
                    break;
                foreach (var item in nextCommunitys)
                {
                    yield return item;
                }
            }
        }
#endif
        // ----- GetCommunitys -----
    }
}
