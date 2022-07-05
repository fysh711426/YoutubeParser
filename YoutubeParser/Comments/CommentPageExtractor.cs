using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using YoutubeParser.Shares;
using YoutubeParser.Utils;

namespace YoutubeParser.Comments
{
    internal class CommentPageExtractor
    {
        private readonly string? _html;

        public CommentPageExtractor(string? html) => _html = html;

        public JObject? TryGetInitialData() => Memo.Cache(this, () =>
            new YoutubePageExtractor(_html).TryGetInitialData()
        );

        private JToken? TryGetVideoContents() => Memo.Cache(this, () =>
            TryGetInitialData()?["contents"]?["twoColumnWatchNextResults"]?["results"]?["results"]?["contents"]
        );

        private JToken? TryGetCommunityContents() => Memo.Cache(this, () =>
            TryGetInitialData()?["contents"]?["twoColumnBrowseResultsRenderer"]?["tabs"]?.Values<JObject>()?
                .Where(it => it?["tabRenderer"]?["selected"]?.Value<bool>() == true)
                .FirstOrDefault()?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]
        );

        public JToken? TryGetContents() => Memo.Cache(this, () =>
            TryGetVideoContents() ?? TryGetCommunityContents()
        );

        public string? TryGetPageContinuation() => Memo.Cache(this, () =>
            TryGetContents()?.Values<JObject>()
                .LastOrDefault()?["itemSectionRenderer"]?["contents"]?.Values<JObject>()
                .LastOrDefault()?["continuationItemRenderer"]?["continuationEndpoint"]?["continuationCommand"]?["token"]?.Value<string>()
        );

        public JToken? TryGetInnerTubeContext() =>
            new YoutubePageExtractor(_html)
                .TryGetYtcfg()?["INNERTUBE_CONTEXT"];

        public string? TryGetContinuation() => _TryGetContinuation();

        private string? _TryGetContinuation(JObject? content = null) => Memo.Cache(this, () =>
            content?["continuationItemRenderer"]?["continuationEndpoint"]?["continuationCommand"]?["token"]?.Value<string>()
        );

        private JToken? TryGetResponseReceived() => Memo.Cache(this, () =>
            new YoutubePageExtractor(_html)
                .TryGetJsonResponse()?["onResponseReceivedEndpoints"]
        );

        // For GetNextCommentsList
        private IEnumerable<JObject?> GetCommentContentsFromNext() => Memo.Cache(this, () =>
            TryGetResponseReceived()?.Values<JObject>()
                .LastOrDefault()?["reloadContinuationItemsCommand"]?["continuationItems"]?.Values<JObject>() ??
            TryGetResponseReceived()?.Values<JObject>()
                .LastOrDefault()?["appendContinuationItemsAction"]?["continuationItems"]?.Values<JObject>() ??
            new List<JObject>()
        );

        public IEnumerable<JToken> GetCommentItemsFromNext()
        {
            var contents = GetCommentContentsFromNext();
            foreach (var content in contents)
            {
                if (content?.ContainsKey("commentThreadRenderer") == true)
                {
                    var comment = content["commentThreadRenderer"];
                    if (comment != null)
                    {
                        yield return comment;
                    }
                    continue;
                }
                _TryGetContinuation(content);
            }
        }
    }
}
