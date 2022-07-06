using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YoutubeParser.Extensions;
using YoutubeParser.Shares;
using YoutubeParser.Utils;

namespace YoutubeParser.Comments
{
    internal class CommentExtractor
    {
        private readonly JToken _content;

        public CommentExtractor(JToken content) => _content = content;

        private JToken? TryGetComment() => Memo.Cache(this, () =>
            _content["comment"]?["commentRenderer"]
        );

        public string GetCommentId() => Memo.Cache(this, () =>
            TryGetComment()?["commentId"]?.Value<string>() ?? ""
        );

        public string GetContent() => Memo.Cache(this, () =>
            TryGetComment()?["contentText"]?["runs"]?.Value<JToken>()?
                .Select(it => it["text"]?.Value<string>())
                .Aggregate(new StringBuilder(), (r, it)=>r.Append(it))
                .ToString() ?? ""
        );

        public bool IsModerated() => Memo.Cache(this, () =>
            _content["isModeratedElqComment"]?.Value<bool>() ?? false
        );

        private string? TryGetPublishedTime() => Memo.Cache(this, () =>
            TryGetComment()?["publishedTimeText"]?["runs"]?.FirstOrDefault()?["text"]?.Value<string>()
        );

        public string GetPublishedTime() => Memo.Cache(this, () =>
            TryGetPublishedTime() ?? ""
        );

        public long GetPublishedTimeSeconds() => Memo.Cache(this, () =>
            TryGetPublishedTime()?.GetPublishedTimeSeconds() ?? 0
        );

        public long GetLikeCount() => Memo.Cache(this, () =>
            TryGetComment()?["voteCount"]?["simpleText"]?.Value<string>()?.GetCountValue() ?? 0
        );

        public string GetAuthorTitle() => Memo.Cache(this, () =>
            TryGetComment()?["authorText"]?["simpleText"]?.Value<string>() ?? ""
        );

        public string GetAuthorChannelId() => Memo.Cache(this, () =>
            TryGetComment()?["authorEndpoint"]?["browseEndpoint"]?["browseId"]?.Value<string>() ?? ""
        );

        public bool GetAuthorIsChannelOwner() => Memo.Cache(this, () =>
            TryGetComment()?["authorIsChannelOwner"]?.Value<bool>() ?? false
        );

        public bool IsPinned() => Memo.Cache(this, () =>
            TryGetComment()?["pinnedCommentBadge"] != null
        );

        public long GetReplyCount() => Memo.Cache(this, () =>
            TryGetComment()?["replyCount"]?.Value<long>() ?? 0
        );

        public string GetAmount() => Memo.Cache(this, () =>
            TryGetComment()?["paidCommentChipRenderer"]?["pdgCommentChipRenderer"]?["chipText"]?["simpleText"]?.Value<string>() ?? ""
        );

        private string GetAmountColorText() => Memo.Cache(this, () =>
            TryGetComment()?["paidCommentChipRenderer"]?["pdgCommentChipRenderer"]?["chipColorPalette"]?["backgroundColor"]?.Value<long?>()?.ToString() ?? ""
        );

        public AmountColor? TryGetAmountColor() => Memo.Cache(this, () =>
            GetAmountColorText().TryGetAmountColor()
        );

        public CommentType GetCommentType() => Memo.Cache(this, () =>
            GetAmount() == "" ? CommentType.Text : CommentType.SuperThanks
        );

        public string? TryGetReplyContinuation() => Memo.Cache(this, () =>
            _content["replies"]?["commentRepliesRenderer"]?["contents"]?.Values<JObject>()
                .FirstOrDefault()?["continuationItemRenderer"]?["continuationEndpoint"]?["continuationCommand"]?["token"]?.Value<string>()
        );

        public List<Thumbnail> GetAuthorThumbnails() => Memo.Cache(this, () =>
            TryGetComment()?["authorThumbnail"]?["thumbnails"]?
                .Values<JObject>()
                .Select(it => new ThumbnailExtractor(it).GetThumbnail())
                .ToList() ?? new List<Thumbnail>()
        );
    }
}
