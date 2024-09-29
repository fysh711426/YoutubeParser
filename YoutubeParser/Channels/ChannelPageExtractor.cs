using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeParser.Extensions;
using YoutubeParser.Shares;
using YoutubeParser.Utils;

namespace YoutubeParser.Channels
{
    internal class ChannelPageExtractor
    {
        private readonly string? _html;

        public ChannelPageExtractor(string? html) => _html = html;

        public JObject? TryGetInitialData() => Memo.Cache(this, () =>
            new YoutubePageExtractor(_html).TryGetInitialData()
        );

        private JToken? TryGetHeader() => Memo.Cache(this, () =>
            TryGetInitialData()?["header"]?["pageHeaderRenderer"]
        );

        private JToken? TryGetHeaderContent() => Memo.Cache(this, () =>
            TryGetHeader()?["content"]?["pageHeaderViewModel"]
        );

        private JObject? TryGetAbout() => Memo.Cache(this, () =>
            TryGetInitialData()?["onResponseReceivedEndpoints"]?
                .FirstOrDefault()?["showEngagementPanelEndpoint"]?["engagementPanel"]?["engagementPanelSectionListRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?
                .FirstOrDefault()?["itemSectionRenderer"]?["contents"]?
                .FirstOrDefault()?["aboutChannelRenderer"]?["metadata"]?["aboutChannelViewModel"]?
                .Value<JObject>()
        );

        public string GetTitle() => Memo.Cache(this, () =>
            TryGetHeader()?["pageTitle"]?.Value<string>() ?? ""
        );

        public string GetChannelId() => Memo.Cache(this, () =>
             TryGetAbout()?["channelId"]?.Value<string>() ?? ""
        );

        public string GetDescription() => Memo.Cache(this, () =>
            TryGetAbout()?["description"]?.Value<string>() ?? ""
        );

        public string GetCanonicalChannelUrl() => Memo.Cache(this, () =>
            TryGetAbout()?["canonicalChannelUrl"]?.Value<string>() ?? ""
        );

        public string GetCountry() => Memo.Cache(this, () =>
            TryGetAbout()?["country"]?.Value<string>() ?? ""
        );

        public long GetSubscriberCount() => Memo.Cache(this, () =>
            TryGetAbout()?["subscriberCountText"]?.Value<string>()?.GetCountValue() ?? 0
        );

        public long GetViewCount() => Memo.Cache(this, () =>
           TryGetAbout()?["viewCountText"]?.Value<string>()?.GetCountValue() ?? 0
        );

        public DateTime GetJoinedDate() => Memo.Cache(this, () =>
            TryGetAbout()?["joinedDateText"]?["content"]?.Value<string>()?.TryGetJoinedDate() ?? default(DateTime)
        );

        public List<Thumbnail> GetThumbnails() => Memo.Cache(this, () =>
            TryGetHeaderContent()?["image"]?["decoratedAvatarViewModel"]?["avatar"]?["avatarViewModel"]?["image"]?["sources"]?
                .Values<JObject>()
                .Select(it => new ThumbnailExtractor(it).GetThumbnail())
                .ToList() ?? new List<Thumbnail>()
        );

        public List<Thumbnail> GetBanners() => Memo.Cache(this, () =>
            TryGetHeaderContent()?["banner"]?["imageBannerViewModel"]?["image"]?["sources"]?
                .Values<JObject>()
                .Select(it => new ThumbnailExtractor(it).GetThumbnail())
                .ToList() ?? new List<Thumbnail>()
        );
    }
}
