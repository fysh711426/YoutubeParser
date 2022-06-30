using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.Commons;
using YoutubeParser.Extensions;
using YoutubeParser.Utils;

namespace YoutubeParser.Videos
{
    internal class VideoPageExtractor
    {
        private readonly string? _html;

        public VideoPageExtractor(string? html) => _html = html;

        public JObject? TryGetInitialData() => Memo.Cache(this, () =>
            new YoutubePageExtractor(_html).TryGetInitialData()
        );

        private JToken? TryGetContents() => Memo.Cache(this, () =>
            TryGetInitialData()?["contents"]?["twoColumnWatchNextResults"]?["results"]?["results"]?["contents"]
        );

        private JObject? TryGetPrimaryInfo() => Memo.Cache(this, () =>
            TryGetContents()?.Values<JObject>()
                .Select(it => it?["videoPrimaryInfoRenderer"]?.Value<JObject>())
                .FirstOrDefault(it => it != null)
        );

        private JObject? TryGetSecondaryInfo() => Memo.Cache(this, () =>
            TryGetContents()?.Values<JObject>()
                .Select(it => it?["videoSecondaryInfoRenderer"]?.Value<JObject>())
                .FirstOrDefault(it => it != null)
        );

        public JObject? TryGetInitialPlayerResponse() => Memo.Cache(this, () =>
            new YoutubePageExtractor(_html).TryGetInitialPlayerResponse()
        );

        public string GetVideoId() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["videoDetails"]?["videoId"]?.Value<string>() ?? ""
        );

        public string GetTitle() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["videoDetails"]?["title"]?.Value<string>() ?? ""
        );

        public TimeSpan? TryGetDuration() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["videoDetails"]?["lengthSeconds"]?.Value<double>()
                .Pipe(it => it != 0 ? (TimeSpan?)TimeSpan.FromSeconds(it) : null)
        );

        public List<string> GetKeywords() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["videoDetails"]?["keywords"]?
                .Values<string>()?.Select(it => it ?? "")?
                .ToList() ?? new List<string>()
        );

        public string GetAuthorTitle() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["videoDetails"]?["author"]?.Value<string>() ?? ""
        );

        public string GetAuthorChannelId() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["videoDetails"]?["channelId"]?.Value<string>() ?? ""
        );

        public string GetDescription() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["videoDetails"]?["shortDescription"]?.Value<string>() ?? ""
        );

        public bool IsPrivate() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["videoDetails"]?["isPrivate"]?.Value<bool>() ?? false
        );

        public List<Thumbnail> GetThumbnails() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["videoDetails"]?["thumbnail"]?["thumbnails"]?
                .Values<JObject>()
                .Select(it => new ThumbnailExtractor(it).GetThumbnail())
                .ToList() ?? new List<Thumbnail>()
        );

        private string? TryGetUploadDate() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["microformat"]?["playerMicroformatRenderer"]?["uploadDate"]?.Value<string>()
        );

        public DateTime GetUploadDate() => Memo.Cache(this, () =>
            GetDateText()
                .Pipe(it => Regex.Match(it, @"([A-Z][a-z]+\s[0-9]+,\s[0-9]+)"))
                .Select(m => m.Groups[1].Value)
                .Pipe(it => it != "" ? it : TryGetUploadDate())?
                .Pipe(it => DateTime.TryParse(it, out var result) ?
                    (DateTime?)result : null) ?? default(DateTime)
                
        );

        public long? TryGetLikeCount() => Memo.Cache(this, () =>
            _html?
                .Pipe(it => Regex.Match(it, @"""label""\s*:\s*""([\d,\.]+) likes"""))
                .Select(m => m.Groups[1].Value)
                .Pipe(it => it.GetCountValue())
        );

        private JToken? TryGetPlayability() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["playabilityStatus"]
        );

        private string GetPlayabilityStatus() => Memo.Cache(this, () =>
            TryGetPlayability()?["status"]?.Value<string>() ?? ""
        );

        public bool IsPlayable() => Memo.Cache(this, () =>
            GetPlayabilityStatus().ToLower() == "ok"
        );

        private string GetDateText() => Memo.Cache(this, () =>
            TryGetPrimaryInfo()?["dateText"]?["simpleText"]?.Value<string>() ?? ""
        );

        private string? TryGetViewCountSimpleText() => Memo.Cache(this, () =>
            TryGetPrimaryInfo()?["viewCount"]?["videoViewCountRenderer"]?["viewCount"]?["simpleText"]?.Value<string>()
        );

        private string? TryGetViewCountRunsText() => Memo.Cache(this, () =>
            TryGetPrimaryInfo()?["viewCount"]?["videoViewCountRenderer"]?["viewCount"]?["runs"]?
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
        
        private bool IsLive() => Memo.Cache(this, () =>
            GetDateText().Contains("Started")
        );

        private bool IsStream() => Memo.Cache(this, () =>
            GetDateText().ToLower().Contains("stream") ||
            GetDateText().Contains("Scheduled")
        );

        private bool IsUpcoming() => Memo.Cache(this, () =>
            GetDateText().Contains("Scheduled") ||
            GetDateText().Contains("Premieres")
        );

        public VideoType GetVideoType() => Memo.Cache(this, () =>
            IsStream() ? VideoType.Stream : VideoType.Video
        );

        public VideoStatus GetVideoStatus() => Memo.Cache(this, () =>
            IsUpcoming() ? VideoStatus.Upcoming : 
                IsLive() ? VideoStatus.Live : VideoStatus.Default
        );
    }
}
