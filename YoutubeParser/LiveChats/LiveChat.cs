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
        internal _LiveChatType _liveChatType { get; set; }
        public LiveChatType LiveChatType { get; set; }
        public string LiveChatId { get; set; } = "";
        public string Message { get; set; } = "";

        /// <summary>
        /// Super member header text.
        /// </summary>
        public string HeaderText { get; set; } = "";

        /// <summary>
        /// Super member header sub text.
        /// </summary>
        public string HeaderSubText { get; set; } = "";

        /// <summary>
        /// Super chat amount.
        /// </summary>
        public string Amount { get; set; } = "";

        /// <summary>
        /// Super chat color type.
        /// </summary>
        public AmountColor? AmountColor { get; set; }

        public string AuthorTitle { get; set; } = "";
        public string AuthorChannelId { get; set; } = "";
        public IReadOnlyList<Thumbnail> AuthorThumbnails { get; set; } = new List<Thumbnail>();

        /// <summary>
        /// Live chat published timestamp text.
        /// </summary>
        public string TimestampText { get; set; } = "";

        /// <summary>
        /// Live chat published timestamp. (usec = sec * 1000 * 1000)
        /// </summary>
        public long TimestampUsec { get; set; }

        /// <summary>
        /// Pinned on top.
        /// </summary>
        public bool IsPinned { get; set; }

        //public string Json { get; set; } = "";
    }
}
