﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.Commons;

namespace YoutubeParser.ChannelVideos
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
        public string PollStatus { get; set; } = "";
        public string ReplyUrl => $"https://www.youtube.com/post/{PostId}";
    }
}