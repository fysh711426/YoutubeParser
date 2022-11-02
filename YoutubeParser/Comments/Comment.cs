using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using YoutubeParser.Shares;

namespace YoutubeParser.Comments
{
    public class Comment
    {
        public CommentType CommentType { get; set; }
        public string CommentId { get; set; } = "";
        public string Content { get; set; } = "";
        public bool IsModerated { get; set; }
        public long LikeCount { get; set; }

        /// <summary>
        /// Comment published time text.
        /// </summary>
        public string PublishedTime { get; set; } = "";

        /// <summary>
        /// Seconds value of PublishedTime, not accurate.
        /// </summary>
        public long PublishedTimeSeconds { get; set; }

        public string AuthorTitle { get; set; } = "";
        public string AuthorChannelId { get; set; } = "";
        public IReadOnlyList<Thumbnail> AuthorThumbnails { get; set; } = new List<Thumbnail>();
        public bool AuthorIsChannelOwner { get; set; }

        /// <summary>
        /// Pinned on top.
        /// </summary>
        public bool IsPinned { get; set; }

        /// <summary>
        /// Super thanks amout.
        /// </summary>
        public string Amount { get; set; } = "";

        /// <summary>
        /// Super thanks color type.
        /// </summary>
        public AmountColor? AmountColor { get; set; }

        /// <summary>
        /// Comment reply count.
        /// </summary>
        public long ReplyCount { get; set; }

        /// <summary>
        /// Comment replies is lazy loading, must use GetRepliesAsync to load.
        /// </summary>
        public List<CommentReply> Replies { get; set; } = new();

        internal string? _continuation { get; set; }
        internal JToken? _context { get; set; }
    }
}
