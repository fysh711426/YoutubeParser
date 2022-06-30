using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.Commons;

namespace YoutubeParser.Videos
{
    public class Video
    {
        public string VideoId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string AuthorTitle { get; set; } = "";
        public string AuthorChannelId { get; set; } = "";
        public VideoType VideoType { get; set; }
        public VideoStatus VideoStatus { get; set; }
        public TimeSpan? Duration { get; set; }
        public IReadOnlyList<Thumbnail> Thumbnails { get; set; } = new List<Thumbnail>();
        public IReadOnlyList<string> Keywords { get; set; } = new List<string>();
        public long ViewCount { get; set; }
        public DateTime UploadDate { get; set; }
        public long? LikeCount { get; set; }
        public string Url => $"https://www.youtube.com/watch?v={VideoId}";
        public string ShortUrl => $"http://youtu.be/{VideoId}";
        public bool IsPrivate { get; set; }
        public bool IsPlayable { get; set; }
    }
}
