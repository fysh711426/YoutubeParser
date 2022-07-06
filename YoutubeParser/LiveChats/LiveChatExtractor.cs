using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YoutubeParser.Extensions;
using YoutubeParser.Shares;
using YoutubeParser.Utils;

namespace YoutubeParser.Comments
{
    internal class LiveChatExtractor
    {
        private readonly JToken _content;

        public LiveChatExtractor(JToken content) => _content = content;

        public string GetJson() => Memo.Cache(this, () =>
            JsonConvert.SerializeObject(_content)
        );

        private JToken? TryGetBanner() => Memo.Cache(this, () =>
            _content["actions"]?.FirstOrDefault()?["addBannerToLiveChatCommand"]?["bannerRenderer"]?["liveChatBannerRenderer"]?["contents"]
        );

        private JToken? TryGetTickerSponsor() => Memo.Cache(this, () =>
            _content["actions"]?.FirstOrDefault()?["addLiveChatTickerItemAction"]?["item"]?
                ["liveChatTickerSponsorItemRenderer"]?["showItemEndpoint"]?["showLiveChatItemEndpoint"]?["renderer"]
        );

        private JToken? TryGetTickerPaid() => Memo.Cache(this, () =>
            _content["actions"]?.FirstOrDefault()?["addLiveChatTickerItemAction"]?["item"]?
                ["liveChatTickerPaidMessageItemRenderer"]?["showItemEndpoint"]?["showLiveChatItemEndpoint"]?["renderer"]
        );

        private JToken? TryGetTickerPaidSticker() => Memo.Cache(this, () =>
            _content["actions"]?.FirstOrDefault()?["addLiveChatTickerItemAction"]?["item"]?
                ["liveChatTickerPaidStickerItemRenderer"]?["showItemEndpoint"]?["showLiveChatItemEndpoint"]?["renderer"]
        );

        private JToken? TryGetChatItem() => Memo.Cache(this, () =>
            _content["actions"]?.FirstOrDefault()?["addChatItemAction"]?["item"]
        );

        private JToken? TryGetChatTextMessage() => Memo.Cache(this, () =>
            TryGetChatItem()?["liveChatTextMessageRenderer"] ??
            TryGetBanner()?["liveChatTextMessageRenderer"]
        );

        private JToken? TryGetChatPaid() => Memo.Cache(this, () =>
            TryGetChatItem()?["liveChatPaidMessageRenderer"] ??
            TryGetChatItem()?["liveChatPaidStickerRenderer"] ??
            TryGetTickerPaid()?["liveChatPaidMessageRenderer"] ??
            TryGetTickerPaidSticker()?["liveChatPaidStickerRenderer"] ??
            TryGetBanner()?["liveChatPaidMessageRenderer"]
        );

        private JToken? TryGetChatMembership() => Memo.Cache(this, () =>
            TryGetChatItem()?["liveChatMembershipItemRenderer"] ??
            TryGetTickerSponsor()?["liveChatMembershipItemRenderer"] ??
            TryGetBanner()?["liveChatMembershipItemRenderer"]
        );

        private JToken? TryGetChatGift() => Memo.Cache(this, () =>
            TryGetTickerSponsor()?["liveChatSponsorshipsGiftPurchaseAnnouncementRenderer"]?["header"]?["liveChatSponsorshipsHeaderRenderer"]?["showItemEndpoint"]?["showItemEndpoint"]
        );

        private JToken? TryGetChatViewerEngagement() => Memo.Cache(this, () =>
            TryGetChatItem()?["liveChatViewerEngagementMessageRenderer"]
        );

        private JToken? TryGetChatPlaceholder() => Memo.Cache(this, () =>
            TryGetChatItem()?["liveChatPlaceholderItemRenderer"]
        );

        public _LiveChatType GetLiveChatType() => Memo.Cache(this, () =>
            TryGetChatViewerEngagement() != null ? _LiveChatType.System :
            TryGetChatPlaceholder() != null ? _LiveChatType.Placeholder :
            TryGetChatTextMessage() != null ? _LiveChatType.Text :
            TryGetChatMembership() != null ? _LiveChatType.Membership :
            TryGetChatPaid() != null ? _LiveChatType.SuperChat :
            TryGetChatGift() != null ? _LiveChatType.Gift :
            _LiveChatType.Unknow
        );

        public bool IsPinned() => Memo.Cache(this, () =>
            TryGetBanner() != null
        );

        private JToken? TryGetChatRenderer() => Memo.Cache(this, () =>
            TryGetChatViewerEngagement() ??
            TryGetChatPlaceholder() ??
            TryGetChatTextMessage() ??
            TryGetChatMembership() ??
            TryGetChatGift() ??
            TryGetChatPaid()
        );

        public string GetLiveChatId() => Memo.Cache(this, () =>
            TryGetChatRenderer()?["id"]?.Value<string>() ?? ""
        );

        private JToken? TryGetHeaderPrimaryText() => Memo.Cache(this, () =>
           TryGetChatRenderer()?["headerPrimaryText"] ??
           TryGetChatRenderer()?["primaryText"]
        );

        public string GetHeaderText() => Memo.Cache(this, () =>
            TryGetHeaderPrimaryText()?["runs"]?
                .Select(it => it?["text"]?.Value<string>())
                .Aggregate(new StringBuilder(), (r, it) => r.Append(it))
                .ToString() ?? ""
        );

        public string GetHeaderSubText() => Memo.Cache(this, () =>
            TryGetChatRenderer()?["headerSubtext"]?["simpleText"]?.Value<string>() ?? ""
        );

        public string GetAmount() => Memo.Cache(this, () =>
            TryGetChatRenderer()?["purchaseAmountText"]?["simpleText"]?.Value<string>() ?? ""
        );

        private string GetAmountColorText() => Memo.Cache(this, () =>
            GetAmount() == "" ? "" :
                TryGetChatRenderer()?["headerBackgroundColor"]?.Value<string>() ??
                TryGetChatRenderer()?["moneyChipBackgroundColor"]?.Value<string>() ?? ""
        );

        public AmountColor? TryGetAmountColor() => Memo.Cache(this, () =>
            GetAmountColorText().TryGetAmountColor()
        );

        public string GetAuthorTitle() => Memo.Cache(this, () =>
            TryGetChatRenderer()?["authorName"]?["simpleText"]?.Value<string>() ?? ""
        );

        public string GetAuthorChannelId() => Memo.Cache(this, () =>
            TryGetChatRenderer()?["authorExternalChannelId"]?.Value<string>() ?? ""
        );

        private string? TryGetEmojiText(JObject? item)
        {
            var isCustomEmoji = item?["emoji"]?["isCustomEmoji"]?.Value<bool>() == true;
            if (!isCustomEmoji)
                return item?["emoji"]?["emojiId"]?.Value<string>();
            return null;
        }

        public string GetMessage() => Memo.Cache(this, () =>
            TryGetChatRenderer()?["message"]?["runs"]?.Values<JObject>()?
                .Select(it =>
                    it?["text"]?.Value<string>() ??
                    TryGetEmojiText(it) ?? "")
                .Aggregate(new StringBuilder(), (r, it) => r.Append(it))
                .ToString() ?? ""
        );

        public string GetTimestampText() => Memo.Cache(this, () =>
            TryGetChatRenderer()?["timestampText"]?["simpleText"]?.Value<string>() ?? ""
        );

        public long GetTimestampUsec() => Memo.Cache(this, () =>
            TryGetChatRenderer()?["timestampUsec"]?.Value<long>() ?? 0
        );

        public List<Thumbnail> GetAuthorThumbnails() => Memo.Cache(this, () =>
            TryGetChatRenderer()?["authorPhoto"]?["thumbnails"]?
                .Values<JObject>()
                .Select(it => new ThumbnailExtractor(it).GetThumbnail())
                .ToList() ?? new List<Thumbnail>()
        );
    }
}
