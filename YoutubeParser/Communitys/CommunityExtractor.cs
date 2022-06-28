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
    internal class CommunityExtractor
    {
        private readonly JToken _content;

        public CommunityExtractor(JToken content) => _content = content;

        public string GetPostId() => Memo.Cache(this, () =>
            _content["postId"]?.Value<string>() ?? ""
        );

        public string GetAuthorTitle() => Memo.Cache(this, () =>
            _content["authorText"]?["runs"]?.FirstOrDefault()?["text"]?.Value<string>() ?? ""
        );

        public string GetAuthorChannelId() => Memo.Cache(this, () =>
            _content["authorEndpoint"]?["browseEndpoint"]?["browseId"]?.Value<string>() ?? ""
        );

        public string GetContent() => Memo.Cache(this, () =>
            _content["contentText"]?["runs"]?
                .Values<JObject>()
                .Select(it => it?["text"]?.Value<string>() ?? "")
                .Aggregate(new StringBuilder(), (r, it) => r.Append(it))
                .ToString() ?? ""
        );

        public long GetLikeCount() => Memo.Cache(this, () =>
            _content["voteCount"]?["accessibility"]?["accessibilityData"]?["label"]?.Value<string>()?.GetCountValue() ?? 0
        );

        public string GetPollStatus() => Memo.Cache(this, () =>
            _content["pollStatus"]?.Value<string>() ?? ""
        );

        public string GetVoteStatus() => Memo.Cache(this, () =>
            _content["voteStatus"]?.Value<string>() ?? ""
        );

        private string? TryGetPublishedTime() => Memo.Cache(this, () =>
            _content["publishedTimeText"]?["runs"]?.FirstOrDefault()?["text"]?.Value<string>()
        );

        public string GetPublishedTime() => Memo.Cache(this, () =>
            TryGetPublishedTime() ?? ""
        );

        public long GetPublishedTimeSeconds() => Memo.Cache(this, () =>
            TryGetPublishedTime()?.GetPublishedTimeSeconds() ?? 0
        );

        private Thumbnail AddScheme(Thumbnail thumbnail)
        {
            thumbnail.Url = $"https:{thumbnail.Url}";
            return thumbnail;
        }

        public List<Thumbnail> GetAuthorThumbnails() => Memo.Cache(this, () =>
            _content["authorThumbnail"]?["thumbnails"]?
                .Values<JObject>()
                .Select(it => new ThumbnailExtractor(it).GetThumbnail())
                .Select(it => AddScheme(it))
                .ToList() ?? new List<Thumbnail>()
        );

        public List<Thumbnail> GetImages() => Memo.Cache(this, () =>
            _content["backstageAttachment"]?["backstageImageRenderer"]?["image"]?["thumbnails"]?
                .Values<JObject>()
                .Select(it => new ThumbnailExtractor(it).GetThumbnail())
                .ToList() ?? new List<Thumbnail>()
        );
    }
}
