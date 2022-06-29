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
                .Pipe(it => TimeSpan.FromSeconds(it))
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

        public DateTime GetUploadDate() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["microformat"]?["playerMicroformatRenderer"]?["uploadDate"]?.Value<string>()?
                .Pipe(it =>
                    DateTime.TryParse(it, out var result)
                        ? (DateTime?)result : null) ?? default(DateTime)
        );

        public JToken? GetliveBroadcast() => Memo.Cache(this, () =>
            TryGetInitialPlayerResponse()?["microformat"]?["playerMicroformatRenderer"]?["liveBroadcastDetails"]
        );

        public long? TryGetLikeCount() => Memo.Cache(this, () =>
            _html?
                .Pipe(it => Regex.Match(it, @"""label""\s*:\s*""([\d,\.]+) likes"""))
                .Select(m => m.Groups[1].Value)
                .Pipe(it => it.GetCountValue())
        );

        public long? TryGetDislikeCount() => Memo.Cache(this, () =>
            _html?
                .Pipe(it => Regex.Match(it, @"""label""\s*:\s*""([\d,\.]+) dislikes"""))
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

        private string GetPublishedTime() => Memo.Cache(this, () =>
            TryGetPrimaryInfo()?["dateText"]?["simpleText"]?.Value<string>() ?? ""
        );

        //public long GetViewCount() => Memo.Cache(this, () =>
        //    TryGetInitialPlayerResponse()?["videoDetails"]?["viewCount"]?.Value<long>() ?? 0
        //);

        private string? TryGetViewCountText() => Memo.Cache(this, () =>
            TryGetPrimaryInfo()?["viewCount"]?["videoViewCountRenderer"]?["viewCount"]?["simpleText"]?.Value<string>()
        );

        private string? TryGetLiveViewCountText() => Memo.Cache(this, () =>
            TryGetPrimaryInfo()?["viewCount"]?["videoViewCountRenderer"]?["viewCount"]?["runs"]?.FirstOrDefault()?["text"]?.Value<string>()
        );

        public long GetViewCount() => Memo.Cache(this, () =>
            TryGetViewCountText()?.GetCountValue() ??
            TryGetLiveViewCountText()?.GetCountValue() ?? 0
        );

        private bool IsLive() => Memo.Cache(this, () =>
            TryGetPrimaryInfo()?["viewCount"]?["videoViewCountRenderer"]?["isLive"]?.Value<bool>() ?? false
        );

        private bool IsStream() => Memo.Cache(this, () =>
            GetPublishedTime().ToLower().Contains("stream") ||
            GetPublishedTime().ToLower().Contains("scheduled")
        );

        private bool IsUpcoming() => Memo.Cache(this, () =>
            GetPublishedTime().ToLower().Contains("scheduled") ||
            GetPublishedTime().ToLower().Contains("premieres")
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
