﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
                ReplyCount = extractor.GetReplyCount()
                //VoteStatus = extractor.GetVoteStatus()
            };
        }

        /// <summary>
        /// Get channel community list by channel url or id.
        /// </summary>
        /// <param name="urlOrChannelId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<Community>> GetCommunitysListAsync(string urlOrChannelId, CancellationToken token = default)
        {
            var url = $"{GetChannelUrl(urlOrChannelId)}/community";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            var html = await response.Content.ReadAsStringAsync();
            var extractor = new CommunityPageExtractor(html);

            token.ThrowIfCancellationRequested();
            var communitys = new List<Community>();
            var communityItems = extractor.GetCommunityItems();
            foreach (var item in communityItems)
            {
                token.ThrowIfCancellationRequested();
                communitys.Add(MapCommunity(item));
            }
            // must be after each GetVideoItems
            _continuationCommunity = extractor.TryGetContinuation();
            _contextCommunity = extractor.TryGetInnerTubeContext();
            return communitys;
        }

        /// <summary>
        /// Get next page channel community list.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<Community>?> GetNextCommunitysListAsync(CancellationToken token = default)
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
            using var response = await client.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            var json = await response.Content.ReadAsStringAsync();
            var extractor = new CommunityPageExtractor(json);

            token.ThrowIfCancellationRequested();
            var communitys = new List<Community>();
            var communityItems = extractor.GetCommunityItemsFromNext();
            foreach (var item in communityItems)
            {
                token.ThrowIfCancellationRequested();
                communitys.Add(MapCommunity(item));
            }
            // must be after each GetCommunityItemsFromNext
            _continuationCommunity = extractor.TryGetContinuation();
            return communitys;
        }

#if (!NET45 && !NET46)
        /// <summary>
        /// Merge get and next method, and add delay between request.
        /// </summary>
        /// <param name="urlOrChannelId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<Community> GetCommunitysAsync(string urlOrChannelId,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var communitys = await GetCommunitysListAsync(urlOrChannelId, token);
            foreach (var item in communitys)
            {
                token.ThrowIfCancellationRequested();
                yield return item;
            }
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (_requestDelay != null)
                    await Task.Delay(_requestDelay(), token);
                var nextCommunitys = await GetNextCommunitysListAsync(token);
                if (nextCommunitys == null)
                    break;
                foreach (var item in nextCommunitys)
                {
                    token.ThrowIfCancellationRequested();
                    yield return item;
                }
            }
        }
#endif
        // ----- GetCommunitys -----
    }
}
