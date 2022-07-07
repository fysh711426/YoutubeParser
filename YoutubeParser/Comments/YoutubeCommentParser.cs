using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using YoutubeParser.Comments;
using YoutubeParser.Shares;

namespace YoutubeParser.Comments
{
    public partial class YoutubeCommentParser : YoutubeParserBase
    {
        public YoutubeCommentParser(HttpClient httpClient)
            : base(httpClient)
        {
        }

        // ----- GetCommentReplies -----
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

        public async Task<List<CommentReply>> GetRepliesListAsync(Comment comment)
        {
            _contextReply = comment._context;
            _continuationReply = comment._continuation;
            if (_continuationReply == null)
                return new List<CommentReply>();
            var comments = await GetNextRepliesListAsync();
            return comments ?? new List<CommentReply>();
        }

        public async Task<List<CommentReply>?> GetNextRepliesListAsync()
        {
            if (_continuationReply == null)
                return null;

            var apiUrl = $"https://www.youtube.com/youtubei/v1/browse?key={apiKey}";
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
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var extractor = new CommentPageExtractor(json);

            var comments = new List<CommentReply>();
            var commentItems = extractor.GetReplyItemsFromNext();
            foreach (var item in commentItems)
            {
                comments.Add(MapComment(item));
            }
            // must be after each GetReplyItemsFromNext
            _continuationReply = extractor.TryGetReplyContinuation();
            return comments;
        }

#if (!NET45 && !NET46)
        public async IAsyncEnumerable<CommentReply> GetRepliesAsync(Comment comment)
        {
            var comments = await GetRepliesListAsync(comment);
            foreach (var item in comments)
            {
                yield return item;
            }
            while (true)
            {
                var nextComments = await GetNextRepliesListAsync();
                if (nextComments == null)
                    break;
                foreach (var item in nextComments)
                {
                    yield return item;
                }
            }
        }
#endif
        // ----- GetCommentReplies -----
    }
}
