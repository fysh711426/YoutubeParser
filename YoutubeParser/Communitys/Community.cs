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
        public IReadOnlyList<Thumbnail> AuthorThumbnails { get; set; } = new List<Thumbnail>();
        public IReadOnlyList<Thumbnail> Images { get; set; } = new List<Thumbnail>();
        public string PublishedTime { get; set; } = "";
        public long PublishedTimeSeconds { get; set; }
        public long LikeCount { get; set; }
        public string VoteStatus { get; set; } = "";
        public long? ReplyCount { get; set; }
        public string Url => $"https://www.youtube.com/post/{PostId}";
    }
}
