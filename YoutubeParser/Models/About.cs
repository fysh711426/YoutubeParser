using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeParser.Models
{
    public class About
    {
        public string Title { get; set; } = "";
        public string Subscriber { get; set; } = "";
        public string ChannelId { get; set; } = "";
        public List<Thumbnail> Thumbnails { get; set; } = new List<Thumbnail>();
        public List<Thumbnail> Banners { get; set; } = new List<Thumbnail>();
        public string Description { get; set; } = "";
        public string ViewCount { get; set; } = "";
        public string JoinedDate { get; set; } = "";
        public string Country { get; set; } = "";
        public string CanonicalChannelUrl { get; set; } = "";
    }
}
