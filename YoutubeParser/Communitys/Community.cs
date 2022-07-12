using System.Collections.Generic;
using YoutubeParser.Shares;

namespace YoutubeParser.Communitys
{
    public class Community
    {
        public string PostId { get; set; } = "";
        public string AuthorTitle { get; set; } = "";
        public string AuthorChannelId { get; set; } = "";
        public string Content { get; set; } = "";
        public long LikeCount { get; set; }
        public IReadOnlyList<Thumbnail> AuthorThumbnails { get; set; } = new List<Thumbnail>();
        public IReadOnlyList<Thumbnail> Images { get; set; } = new List<Thumbnail>();

        /// <summary>
        /// Community published time text.
        /// </summary>
        public string PublishedTime { get; set; } = "";

        /// <summary>
        /// Seconds value of PublishedTime, not accurate.
        /// </summary>
        public long PublishedTimeSeconds { get; set; }

        /// <summary>
        /// Community reply count.
        /// </summary>
        public long? ReplyCount { get; set; }

        /// <summary>
        /// https://www.youtube.com/post/{postId}
        /// </summary>
        public string Url => $"https://www.youtube.com/post/{PostId}";
        //public string VoteStatus { get; set; } = "";
    }
}
