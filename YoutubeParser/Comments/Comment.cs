﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeParser.Shares;

namespace YoutubeParser.Comments
{
    public class Comment
    {
        public string CommentId { get; set; } = "";
        public string Content { get; set; } = "";
        public bool IsModerated { get; set; }
        public string PublishedTime { get; set; } = "";
        public long PublishedTimeSeconds { get; set; }
        public long LikeCount { get; set; }
        public string AuthorTitle { get; set; } = "";
        public string AuthorChannelId { get; set; } = "";
        public IReadOnlyList<Thumbnail> AuthorThumbnails { get; set; } = new List<Thumbnail>();
        public bool AuthorIsChannelOwner { get; set; }
        public bool IsPinned { get; set; }
        public long ReplyCount { get; set; }
        public List<Comment> Replies { get; set; } = new List<Comment>();
        public string? ReplyContinuation { get; set; }
    }
}