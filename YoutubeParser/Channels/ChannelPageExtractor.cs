using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeParser.Commons;
using YoutubeParser.Extensions;
using YoutubeParser.Utils;

namespace YoutubeParser.Channels
{
    internal class ChannelPageExtractor
    {
        private readonly JObject? _content;

        public ChannelPageExtractor(JObject? content) => _content = content;

        private JToken? TryGetHeader() => Memo.Cache(this, () =>
            _content?["header"]?["c4TabbedHeaderRenderer"]
        );

        private JToken? TryGetTabs() => Memo.Cache(this, () =>
            _content?["contents"]?["twoColumnBrowseResultsRenderer"]?["tabs"]
        );

        public JObject? TryGetSelectedTab() => Memo.Cache(this, () =>
            TryGetTabs()?.Values<JObject>()
                .Where(it => it?["tabRenderer"]?["selected"]?.Value<bool>() == true)
                .FirstOrDefault()
        );

        private JObject? TryGetAbout() => Memo.Cache(this, () =>
            TryGetSelectedTab()?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?
                .FirstOrDefault()?["itemSectionRenderer"]?["contents"]?
                .FirstOrDefault()?["channelAboutFullMetadataRenderer"]?
                .Value<JObject>()
        );

        public string GetTitle() => Memo.Cache(this, () =>
            TryGetHeader()?["title"]?.Value<string>() ?? ""
        );

        public string GetChannelId() => Memo.Cache(this, () =>
             TryGetHeader()?["channelId"]?.Value<string>() ?? ""
        );

        public string GetDescription() => Memo.Cache(this, () =>
            TryGetAbout()?["description"]?["simpleText"]?.Value<string>() ?? ""
        );

        public string GetCanonicalChannelUrl() => Memo.Cache(this, () =>
            TryGetAbout()?["canonicalChannelUrl"]?.Value<string>() ?? ""
        );

        public string GetCountry() => Memo.Cache(this, () =>
            TryGetAbout()?["country"]?["simpleText"]?.Value<string>() ?? ""
        );

        public long GetSubscriberCount() => Memo.Cache(this, () =>
            TryGetHeader()?["subscriberCountText"]?["simpleText"]?.Value<string>()?.GetCountValue() ?? 0
        );

        public long GetViewCount() => Memo.Cache(this, () =>
           TryGetAbout()?["viewCountText"]?["simpleText"]?.Value<string>()?.GetCountValue() ?? 0
        );

        public DateTime GetJoinedDate() => Memo.Cache(this, () =>
            TryGetAbout()?["joinedDateText"]?["runs"]?.LastOrDefault()?["text"]?.Value<string>()?.TryGetJoinedDate() ?? default(DateTime)
        );

        public List<Thumbnail> GetThumbnails() => Memo.Cache(this, () =>
            TryGetHeader()?["avatar"]?["thumbnails"]?
                .Values<JObject>()
                .Select(it => new ThumbnailExtractor(it).GetThumbnail())
                .ToList() ?? new List<Thumbnail>()
        );

        public List<Thumbnail> GetBanners() => Memo.Cache(this, () =>
            TryGetHeader()?["banner"]?["thumbnails"]?
                .Values<JObject>()
                .Select(it => new ThumbnailExtractor(it).GetThumbnail())
                .ToList() ?? new List<Thumbnail>()
        );
    }
}
