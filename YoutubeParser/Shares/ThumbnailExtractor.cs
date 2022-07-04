using Newtonsoft.Json.Linq;
using YoutubeParser.Utils;

namespace YoutubeParser.Shares
{
    internal class ThumbnailExtractor
    {
        private readonly JObject? _content;

        public ThumbnailExtractor(JObject? content) => _content = content;

        public string? TryGetUrl() => Memo.Cache(this, () =>
            _content?["url"]?.Value<string>()
        );

        public int? TryGetWidth() => Memo.Cache(this, () =>
            _content?["width"]?.Value<int>()
        );

        public int? TryGetHeight() => Memo.Cache(this, () =>
            _content?["height"]?.Value<int>()
        );

        public Thumbnail GetThumbnail() => Memo.Cache(this, () =>
            new Thumbnail
            {
                Url = TryGetUrl() ?? "",
                Width = TryGetWidth() ?? 0,
                Height = TryGetHeight() ?? 0,
            }
        );
    }
}
