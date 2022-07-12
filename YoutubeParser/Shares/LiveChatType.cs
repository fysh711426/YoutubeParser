using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeParser.Shares
{
    internal enum _LiveChatType
    {
        Text,
        SuperChat,
        Gift,
        Membership,
        Unknow,
        System,
        Placeholder
    }

    public enum LiveChatType
    {
        /// <summary>
        /// Text message.
        /// </summary>
        Text,

        /// <summary>
        /// Super chat or super sticker.
        /// </summary>
        SuperChat,

        /// <summary>
        /// Give member gift or receive member gift.
        /// </summary>
        Gift,

        /// <summary>
        /// Member join or member chat.
        /// </summary>
        Membership
    }
}
