using System;
using System.Collections.Generic;
using YoutubeParser.Shares;

namespace YoutubeParser.ChannelVideos
{
    public class ChannelVideo
    {
        public string VideoId { get; set; } = "";
        public string Title { get; set; } = "";
        public VideoType VideoType { get; set; }
        public VideoStatus VideoStatus { get; set; }
        public long ViewCount { get; set; }
        public TimeSpan? Duration { get; set; }
        public IReadOnlyList<Thumbnail> Thumbnails { get; set; } = new List<Thumbnail>();

        /// <summary>
        /// Short video.
        /// </summary>
        public bool IsShorts { get; set; }
        
        /// <summary>
        /// Has time limit, need to save the picture.
        /// </summary>
        public Thumbnail? RichThumbnail { get; set; }

        /// <summary>
        /// Video published time text.
        /// </summary>
        public string PublishedTime { get; set; } = "";

        /// <summary>
        /// Seconds value of PublishedTime, not accurate.
        /// </summary>
        public long PublishedTimeSeconds { get; set; }

        /// <summary>
        /// Video scheduled or premieres date time.
        /// </summary>
        public DateTime? UpcomingDate { get; set; }

        /// <summary>
        /// https://www.youtube.com/watch?v={videoId}
        /// </summary>
        public string Url => $"https://www.youtube.com/watch?v={VideoId}";

        /// <summary>
        /// http://youtu.be/{videoId}
        /// </summary>
        public string ShortUrl => $"http://youtu.be/{VideoId}";
    }
}
