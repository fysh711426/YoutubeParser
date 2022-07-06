using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeParser.Shares;

namespace YoutubeParser.LiveChats
{
    public class LiveChat
    {
        public LiveChatType LiveChatType { get; set; }
        public string LiveChatId { get; set; } = "";
        public string Message { get; set; } = "";
        public string HeaderText { get; set; } = "";
        public string HeaderSubText { get; set; } = "";
        public string Amount { get; set; } = "";
        public AmountColor? AmountColor { get; set; }
        public string AuthorTitle { get; set; } = "";
        public string AuthorChannelId { get; set; } = "";
        public IReadOnlyList<Thumbnail> AuthorThumbnails { get; set; } = new List<Thumbnail>();
        public string TimestampText { get; set; } = "";
        public long TimestampUsec { get; set; }
        public string Json { get; set; } = "";
    }
}
