using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeParser.Models
{
    public class Video
    {
        public string VideoId { get; set; } = "";
        public string Title { get; set; } = "";
        public string PublishedTime { get; set; } = "";
        public string ViewCount { get; set; } = "";
        public string ShortViewCount { get; set; } = "";
        public string Duration { get; set; } = "";
        public List<Thumbnail> Thumbnails { get; set; } = new List<Thumbnail>();
        public List<Thumbnail> RichThumbnails { get; set; } = new List<Thumbnail>();
    }
}
