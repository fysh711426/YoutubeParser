using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeParser.Extensions;
using YoutubeParser.Models;
using YoutubeParser.Utils;

namespace YoutubeParser.ChannelVideos
{
    internal class ChannelVideoExtractor
    {
        private readonly JToken _content;

        public ChannelVideoExtractor(JToken content) => _content = content;

        private JToken? TryGetThumbnailOverlayTimeStatus() => Memo.Cache(this, () =>
            _content["thumbnailOverlays"]?.FirstOrDefault()?["thumbnailOverlayTimeStatusRenderer"]
        );

        private string? TryGetDurationText() => Memo.Cache(this, () =>
            TryGetThumbnailOverlayTimeStatus()?["text"]?["simpleText"]?.Value<string>()
        );

        private string? TryGetPublishedTime() => Memo.Cache(this, () =>
            _content["publishedTimeText"]?["simpleText"]?.Value<string>()
        );

        private string? TryGetViewCountText() => Memo.Cache(this, () =>
            _content["viewCountText"]?["simpleText"]?.Value<string>()
        );

        private string? TryGetLiveViewCountText() => Memo.Cache(this, () =>
            _content["viewCountText"]?["runs"]?.FirstOrDefault()?["text"]?.Value<string>()
        );

        private bool IsLive() => Memo.Cache(this, () =>
            TryGetThumbnailOverlayTimeStatus()?["style"]?.Value<string>() == "LIVE"
        );

        private bool IsStream() => Memo.Cache(this, () =>
            TryGetPublishedTime()?.Contains("Streamed") ?? false
        );

        public bool IsShorts() => Memo.Cache(this, () =>
            TryGetDurationText() == "SHORTS"
        );

        public string GetTitle() => Memo.Cache(this, () =>
            _content["title"]?["runs"]?.FirstOrDefault()?["text"]?.Value<string>() ?? ""
        );

        public string GetVideoId() => Memo.Cache(this, () =>
            _content["videoId"]?.Value<string>() ?? ""
        );

        public TimeSpan? TryGetDuration() => Memo.Cache(this, () =>
            !IsShorts() ? TryGetDurationText()?.TryGetDuration() : null
        );

        public string GetPublishedTime() => Memo.Cache(this, () =>
            TryGetPublishedTime() ?? ""
        );

        public long GetPublishedTimeSeconds() => Memo.Cache(this, () =>
            TryGetPublishedTime()?.GetPublishedTimeSeconds() ?? 0
        );

        public long GetViewCount() => Memo.Cache(this, () =>
            TryGetViewCountText()?.GetCountValue() ??
            TryGetLiveViewCountText()?.GetCountValue() ?? 0
        );

        public VideoType GetVideoType() => Memo.Cache(this, () =>
            IsLive() || IsStream() ? VideoType.Stream : VideoType.Video
        );

        public VideoStatus GetVideoStatus() => Memo.Cache(this, () =>
           IsLive() ? VideoStatus.Live : VideoStatus.Default
        );

        private Thumbnail GetThumbnail(JObject? data) => new Thumbnail
        {
            Url = data?["url"]?.Value<string>() ?? "",
            Width = data?["width"]?.Value<int>() ?? 0,
            Height = data?["height"]?.Value<int>() ?? 0
        };

        public List<Thumbnail> GetThumbnails() => Memo.Cache(this, () =>
            _content["thumbnail"]?["thumbnails"]?
                .Values<JObject>()
                .Select(it => GetThumbnail(it))
                .ToList() ?? new List<Thumbnail>()
        );

        public Thumbnail? TryGetRichThumbnail() => Memo.Cache(this, () =>
            _content["richThumbnail"]?["movingThumbnailRenderer"]?["movingThumbnailDetails"]?["thumbnails"]?
                .Values<JObject>()
                .Select(it => GetThumbnail(it))
                .FirstOrDefault()
        );
    }
}
