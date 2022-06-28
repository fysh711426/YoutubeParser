using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.Models;

namespace YoutubeParser.ChannelVideos
{
    public class Community
    {
        public string PostId { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string AuthorChannelId { get; set; } = "";
        public string Content { get; set; } = "";
        public List<Thumbnail> AuthorThumbnails { get; set; } = new();
        public List<Thumbnail> Images { get; set; } = new();
        public string PublishedTime { get; set; } = "";
        public long PublishedTimeSeconds { get; set; }
        public long LikeCount { get; set; }
        public string VoteStatus { get; set; } = "";
        public string PollStatus { get; set; } = "";
        public string ReplyUrl => $"https://www.youtube.com/post/{PostId}";
    }
}
