using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.Models;

namespace YoutubeParser.Channels
{
    public class Channel
    {
        public string Title { get; set; } = "";
        public string ChannelId { get; set; } = "";
        public List<Thumbnail> Thumbnails { get; set; } = new List<Thumbnail>();
        public List<Thumbnail> Banners { get; set; } = new List<Thumbnail>();
        public string Description { get; set; } = "";
        public DateTime? JoinedDate { get; set; }
        public string Country { get; set; } = "";
        public string CanonicalChannelUrl { get; set; } = "";
        public long ViewCount { get; set; }
        public long SubscriberCount { get; set; }
        public string Url => $"https://www.youtube.com/channel/{ChannelId}";
    }
}
