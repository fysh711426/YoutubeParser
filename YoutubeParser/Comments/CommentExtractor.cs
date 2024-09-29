using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using YoutubeParser.Extensions;
using YoutubeParser.Shares;
using YoutubeParser.Utils;

namespace YoutubeParser.Comments
{
    internal class CommentExtractor
    {
        private readonly JToken _content;

        public CommentExtractor(JToken content) => _content = content;

        //private JToken? TryGetCommentReply() => Memo.Cache(this, () =>
        //    _content["commentRenderer"]
        //);

        //private JToken? TryGetComment() => Memo.Cache(this, () =>
        //    _content["comment"]?["commentRenderer"] ?? TryGetCommentReply()
        //);

        public string GetCommentId() => Memo.Cache(this, () =>
            _content["properties"]?["commentId"]?.Value<string>() ?? ""
        );

        public string GetContent() => Memo.Cache(this, () =>
            _content["properties"]?["content"]?["content"]?.Value<string>() ?? ""
        );

        public bool IsModerated() => Memo.Cache(this, () =>
            //_content["isModeratedElqComment"]?.Value<bool>() ?? false
            false
        );

        private string? TryGetPublishedTime() => Memo.Cache(this, () =>
            _content["properties"]?["publishedTime"]?.Value<string>()
        );

        public string GetPublishedTime() => Memo.Cache(this, () =>
            TryGetPublishedTime() ?? ""
        );

        public long GetPublishedTimeSeconds() => Memo.Cache(this, () =>
            TryGetPublishedTime()?.GetPublishedTimeSeconds() ?? 0
        );

        public long GetLikeCount() => Memo.Cache(this, () =>
            _content["toolbar"]?["likeCountNotliked"]?.Value<string>()?.GetCountValue() ?? 0
        );

        public string GetAuthorTitle() => Memo.Cache(this, () =>
            _content["author"]?["displayName"]?.Value<string>() ?? ""
        );

        public string GetAuthorChannelId() => Memo.Cache(this, () =>
            _content["author"]?["channelId"]?.Value<string>() ?? ""
        );

        public bool GetAuthorIsChannelOwner() => Memo.Cache(this, () =>
            _content["author"]?["isCreator"]?.Value<bool>() ?? false
        );

        public bool IsPinned() => Memo.Cache(this, () =>
            //TryGetComment()?["pinnedCommentBadge"] != null
            false
        );

        public string GetReplyCountText() => Memo.Cache(this, () =>
            _content["toolbar"]?["replyCount"]?.Value<string>() ?? ""
        );

        public long GetReplyCount() => Memo.Cache(this, () =>
            GetReplyCountText().GetCountValue()
        );

        public string GetAmount() => Memo.Cache(this, () =>
            //TryGetComment()?["paidCommentChipRenderer"]?["pdgCommentChipRenderer"]?["chipText"]?["simpleText"]?.Value<string>() ?? ""
            ""
        );

        private string GetAmountColorText() => Memo.Cache(this, () =>
            //TryGetComment()?["paidCommentChipRenderer"]?["pdgCommentChipRenderer"]?["chipColorPalette"]?["backgroundColor"]?.Value<long?>()?.ToString() ?? ""
            ""
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
            _content["avatar"]?["image"]?["sources"]?
                .Values<JObject>()
                .Select(it => new ThumbnailExtractor(it).GetThumbnail())
                .ToList() ?? new List<Thumbnail>()
        );
    }
}
