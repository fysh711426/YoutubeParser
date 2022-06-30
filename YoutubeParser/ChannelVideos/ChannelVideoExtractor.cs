using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeParser.Commons;
using YoutubeParser.Extensions;
using YoutubeParser.Utils;

namespace YoutubeParser.ChannelVideos
{
    internal class ChannelVideoExtractor
    {
        private readonly JToken _content;

        public ChannelVideoExtractor(JToken content) => _content = content;

        private JToken? TryGetOverlayTimeStatus() => Memo.Cache(this, () =>
            _content["thumbnailOverlays"]?.FirstOrDefault()?["thumbnailOverlayTimeStatusRenderer"]
        );

        private string GetOverlayTimeStatusText() => Memo.Cache(this, () =>
            TryGetOverlayTimeStatus()?["text"]?["accessibility"]?["accessibilityData"]?["label"]?.Value<string>() ?? ""
        );

        private string GetOverlayTimeStatusStyle() => Memo.Cache(this, () =>
            TryGetOverlayTimeStatus()?["style"]?.Value<string>() ?? ""
        );

        private string? TryGetDurationText() => Memo.Cache(this, () =>
            TryGetOverlayTimeStatus()?["text"]?["simpleText"]?.Value<string>()
        );

        private string? TryGetPublishedTime() => Memo.Cache(this, () =>
            _content["publishedTimeText"]?["simpleText"]?.Value<string>()
        );

        public string? TryGetUpcomingStartTimeText() => Memo.Cache(this, () =>
            _content["upcomingEventData"]?["startTime"]?.Value<string>()
        );

        public DateTime? TryGetUpcomingDate() => Memo.Cache(this, () =>
            TryGetUpcomingStartTimeText()?
                .Pipe(it => long.TryParse(it, out var result) ?
                    (long?)result : null)?
                        .Pipe(it => DateTime.Parse("1970/01/01")
                            .AddSeconds(it).ToLocalTime())
        );

        public string GetUpcomingEvent() => Memo.Cache(this, () =>
            _content["upcomingEventData"]?["upcomingEventText"]?["runs"]?.FirstOrDefault()?["text"]?.Value<string>() ?? ""
        );

        private string? TryGetViewCountSimpleText() => Memo.Cache(this, () =>
            _content["viewCountText"]?["simpleText"]?.Value<string>()
        );

        private string? TryGetViewCountRunsText() => Memo.Cache(this, () =>
            _content["viewCountText"]?["runs"]?
                .Select(it => it?["text"]?.Value<string>() ?? "")
                .Aggregate(new StringBuilder(), (r, it) => r.Append(it))
                .ToString()
        );

        private string GetViewCountText() => Memo.Cache(this, () =>
            TryGetViewCountSimpleText() ?? TryGetViewCountRunsText() ?? ""
        );

        public long GetViewCount() => Memo.Cache(this, () =>
            GetViewCountText().GetCountValue()
        );

        private bool IsStream() => Memo.Cache(this, () =>
            GetPublishedTime().ToLower().Contains("stream") ||
            GetUpcomingEvent().Contains("Scheduled") ||
            GetOverlayTimeStatusText() == "LIVE"
        );

        private bool IsLive() => Memo.Cache(this, () =>
            GetOverlayTimeStatusStyle() == "LIVE"
        );

        private bool IsUpcoming() => Memo.Cache(this, () =>
            GetOverlayTimeStatusStyle() == "UPCOMING"
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

        public VideoType GetVideoType() => Memo.Cache(this, () =>
            IsStream() ? VideoType.Stream : VideoType.Video
        );

        public VideoStatus GetVideoStatus() => Memo.Cache(this, () =>
            IsUpcoming() ? VideoStatus.Upcoming :
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
