using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.Commons;

namespace YoutubeParser.ChannelVideos
{
    public class ChannelVideo
    {
        public string VideoId { get; set; } = "";
        public string Title { get; set; } = "";
        public VideoType VideoType { get; set; }
        public VideoStatus VideoStatus { get; set; }
        public bool IsShorts { get; set; }
        public TimeSpan? Duration { get; set; }
        public IReadOnlyList<Thumbnail> Thumbnails { get; set; } = new List<Thumbnail>();
        public Thumbnail? RichThumbnail { get; set; }
        public long ViewCount { get; set; }
        public string PublishedTime { get; set; } = "";
        public long PublishedTimeSeconds { get; set; }
        public string Url => $"https://www.youtube.com/watch?v={VideoId}";
        public string ShortUrl => $"http://youtu.be/{VideoId}";
    }
}
