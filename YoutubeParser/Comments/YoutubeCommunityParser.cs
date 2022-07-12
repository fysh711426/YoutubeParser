using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeParser.Comments;

namespace YoutubeParser.Communitys
{
    public partial class YoutubeCommunityParser
    {
        // ----- GetComments -----
        private string? _continuationComment;
        private JToken? _contextComment;

        private Comment MapComment(JToken content)
        {
            var extractor = new CommentExtractor(content);
            return new Comment
            {
                CommentId = extractor.GetCommentId(),
                Content = extractor.GetContent(),
                IsModerated = extractor.IsModerated(),
                PublishedTime = extractor.GetPublishedTime(),
                PublishedTimeSeconds = extractor.GetPublishedTimeSeconds(),
                LikeCount = extractor.GetLikeCount(),
                AuthorTitle = extractor.GetAuthorTitle(),
                AuthorChannelId = extractor.GetAuthorChannelId(),
                AuthorThumbnails = extractor.GetAuthorThumbnails(),
                AuthorIsChannelOwner = extractor.GetAuthorIsChannelOwner(),
                IsPinned = extractor.IsPinned(),
                ReplyCount = extractor.GetReplyCount(),
                _continuation = extractor.TryGetReplyContinuation(),
                _context = _contextComment
            };
        }

        /// <summary>
        /// Get community comment list by community url or id.
        /// </summary>
        /// <param name="urlOrCommunityId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<Comment>> GetCommentsListAsync(string urlOrCommunityId, CancellationToken token = default)
        {
            var url = $"{GetCommunityUrl(urlOrCommunityId)}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            var html = await response.Content.ReadAsStringAsync();
            var extractor = new CommentPageExtractor(html);
            _continuationComment = extractor.TryGetPageContinuation();
            _contextComment = extractor.TryGetInnerTubeContext();
            if (_continuationComment == null)
                return new List<Comment>();
            token.ThrowIfCancellationRequested();
            var comments = await GetNextCommentsListAsync(token);
            return comments ?? new List<Comment>();
        }

        /// <summary>
        /// Get next page community comment list.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<Comment>?> GetNextCommentsListAsync(CancellationToken token = default)
        {
            if (_continuationComment == null)
                return null;

            var apiUrl = $"https://www.youtube.com/youtubei/v1/browse?key={apiKey}";
            var client = _httpClient;

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            var payload = new
            {
                context = _contextComment,
                continuation = _continuationComment
            };
            var content = new StringContent(
                JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");
            request.Content = content;
            using var response = await client.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
            var json = await response.Content.ReadAsStringAsync();
            var extractor = new CommentPageExtractor(json);

            token.ThrowIfCancellationRequested();
            var comments = new List<Comment>();
            var commentItems = extractor.GetCommentItemsFromNext();
            foreach (var item in commentItems)
            {
                token.ThrowIfCancellationRequested();
                comments.Add(MapComment(item));
            }
            // must be after each GetCommentItemsFromNext
            _continuationComment = extractor.TryGetContinuation();
            return comments;
        }

#if (!NET45 && !NET46)
        /// <summary>
        /// Merge get and next method, and add delay between request.
        /// </summary>
        /// <param name="urlOrCommunityId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<Comment> GetCommentsAsync(string urlOrCommunityId,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var comments = await GetCommentsListAsync(urlOrCommunityId, token);
            foreach (var item in comments)
            {
                token.ThrowIfCancellationRequested();
                yield return item;
            }
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (_requestDelay != null)
                    await Task.Delay(_requestDelay(), token);
                var nextComments = await GetNextCommentsListAsync(token);
                if (nextComments == null)
                    break;
                foreach (var item in nextComments)
                {
                    token.ThrowIfCancellationRequested();
                    yield return item;
                }
            }
        }
#endif
        // ----- GetComments -----
    }
}
