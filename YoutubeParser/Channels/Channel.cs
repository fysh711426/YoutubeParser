﻿using System;
using System.Collections.Generic;
using YoutubeParser.Shares;

namespace YoutubeParser.Channels
{
    public class Channel
    {
        public string Title { get; set; } = "";
        public string ChannelId { get; set; } = "";
        public IReadOnlyList<Thumbnail> Thumbnails { get; set; } = new List<Thumbnail>();
        public IReadOnlyList<Thumbnail> Banners { get; set; } = new List<Thumbnail>();
        public string Description { get; set; } = "";
        public DateTime? JoinedDate { get; set; }
        public string Country { get; set; } = "";
        public long ViewCount { get; set; }
        public long SubscriberCount { get; set; }

        /// <summary>
        /// https://www.youtube.com/c/{userDefined}
        /// </summary>
        public string CanonicalChannelUrl { get; set; } = "";

        /// <summary>
        /// https://www.youtube.com/channel/{channelId}
        /// </summary>
        public string Url => $"https://www.youtube.com/channel/{ChannelId}";
    }
}
