﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeParser.Comments;

namespace YoutubeParser.Videos
{
    public partial class YoutubeVideoParser
    {
        // ----- GetComments -----
        private string? _continuationComment;
        private JToken? _contextComment;

        private VideoComment MapComment(JToken content)
        {
            var extractor = new CommentExtractor(content);
            return new VideoComment
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
                CommentType = extractor.GetCommentType(),
                Amount = extractor.GetAmount(),
                AmountColor = extractor.TryGetAmountColor(),
                _continuation = extractor.TryGetReplyContinuation(),
                _context = _contextComment
            };
        }

        /// <summary>
        /// Get video comment list by video url or id.
        /// </summary>
        /// <param name="urlOrVideoId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<VideoComment>> GetCommentsListAsync(string urlOrVideoId, CancellationToken token = default)
        {
            var url = $"{GetVideoUrl(urlOrVideoId)}";
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
                return new List<VideoComment>();
            token.ThrowIfCancellationRequested();
            var comments = await GetNextCommentsListAsync(token);
            return comments ?? new List<VideoComment>();
        }

        /// <summary>
        /// Get next page video comment list.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<List<VideoComment>?> GetNextCommentsListAsync(CancellationToken token = default)
        {
            if (_continuationComment == null)
                return null;

            var apiUrl = $"https://www.youtube.com/youtubei/v1/next?key={apiKey}";
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
            var comments = new List<VideoComment>();
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
        public async IAsyncEnumerable<VideoComment> GetCommentsAsync(string urlOrCommunityId,
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
