using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.Models;

namespace YoutubeParser.Channels
{
    public class ChannelVideo
    {
        public string VideoId { get; set; } = "";
        public string Title { get; set; } = "";
        public TimeSpan Duration { get; set; }
        public List<Thumbnail> Thumbnails { get; set; } = new List<Thumbnail>();
        public List<Thumbnail> RichThumbnails { get; set; } = new List<Thumbnail>();
        public long ViewCount { get; set; }
        public string PublishedTime { get; set; } = "";
        public long PublishedTimeSeconds { get; set; }
        public bool IsStream { get; set; }
        public bool IsLive { get; set; }
    }
}
