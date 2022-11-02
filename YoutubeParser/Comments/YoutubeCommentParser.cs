using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeParser.Shares;

namespace YoutubeParser.Comments
{
    public partial class YoutubeCommentParser : YoutubeParserBase
    {
        public YoutubeCommentParser(
            HttpClient httpClient, Func<int>? requestDelay)
                : base(httpClient, requestDelay)
        {
        }

        // ----- GetCommentReplies -----

        private enum CommentType
        {
            Video,
            Community
        }
        private CommentType _commentType;
        private string? _continuationReply;
        private JToken? _contextReply;

        private CommentReply MapComment(JToken content)
        {
            var extractor = new CommentExtractor(content);
            return new CommentReply
            {
                CommentId = extractor.GetCommentId(),
                Content = extractor.GetContent(),
                PublishedTime = extractor.GetPublishedTime(),
                PublishedTimeSeconds = extractor.GetPublishedTimeSeconds(),
                LikeCount = extractor.GetLikeCount(),
                AuthorTitle = extractor.GetAuthorTitle(),
                AuthorChannelId = extractor.GetAuthorChannelId(),
                AuthorThumbnails = extractor.GetAuthorThumbnails(),
                AuthorIsChannelOwner = extractor.GetAuthorIsChannelOwner()
            };
        }

        /// <summary>
        /// Get video comment reply list by comment object.
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<CommentReply>> GetRepliesListAsync(VideoComment comment, CancellationToken token = default)
        {
            _commentType = CommentType.Video;
            return await _GetRepliesListAsync(comment, token);
        }

        /// <summary>
        /// Get community comment reply list by comment object.
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<CommentReply>> GetRepliesListAsync(CommunityComment comment, CancellationToken token = default)
        {
            _commentType = CommentType.Community;
            return await _GetRepliesListAsync(comment, token);
        }

        private async Task<List<CommentReply>> _GetRepliesListAsync(Comment comment, CancellationToken token = default)
        {
            _contextReply = comment._context;
            _continuationReply = comment._continuation;
            if (_continuationReply == null)
                return new List<CommentReply>();

            token.ThrowIfCancellationRequested();
            var comments = await GetNextRepliesListAsync(token);
            return comments ?? new List<CommentReply>();
        }

        /// <summary>
        /// Get next page comment reply list.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<CommentReply>?> GetNextRepliesListAsync(CancellationToken token = default)
        {
            if (_continuationReply == null)
                return null;
            
            var apiUrl = $"https://www.youtube.com/youtubei/v1/browse?key={apiKey}";
            if (_commentType == CommentType.Video)
                apiUrl = $"https://www.youtube.com/youtubei/v1/next?key={apiKey}";
            var client = _httpClient;

            using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
            var payload = new
            {
                context = _contextReply,
                continuation = _continuationReply
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
            var comments = new List<CommentReply>();
            var commentItems = extractor.GetReplyItemsFromNext();
            foreach (var item in commentItems)
            {
                token.ThrowIfCancellationRequested();
                comments.Add(MapComment(item));
            }
            // must be after each GetReplyItemsFromNext
            _continuationReply = extractor.TryGetReplyContinuation();
            return comments;
        }

#if (!NET45 && !NET46)
        /// <summary>
        /// Merge get and next method, and add delay between request.
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<CommentReply> GetRepliesAsync(VideoComment comment,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var comments = await GetRepliesListAsync(comment, token);
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
                var nextComments = await GetNextRepliesListAsync(token);
                if (nextComments == null)
                    break;
                foreach (var item in nextComments)
                {
                    token.ThrowIfCancellationRequested();
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Merge get and next method, and add delay between request.
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async IAsyncEnumerable<CommentReply> GetRepliesAsync(CommunityComment comment,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var comments = await GetRepliesListAsync(comment, token);
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
                var nextComments = await GetNextRepliesListAsync(token);
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
        // ----- GetCommentReplies -----
    }
}
