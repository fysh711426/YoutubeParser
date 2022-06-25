using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeParser.Models
{
    public class Info
    {
        public string Title { get; set; } = "";
        public string Subscriber { get; set; } = "";
        public List<Thumbnail> Thumbnails { get; set; } = new List<Thumbnail>();
    }
}
