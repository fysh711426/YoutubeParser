using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeParser.Utils;

namespace YoutubeParser.ChannelVideos
{
    internal class CommunityPageExtractor
    {
        private readonly JObject? _content;

        public CommunityPageExtractor(JObject? content) => _content = content;

        private IEnumerable<JObject?> GetCommunityContents() => Memo.Cache(this, () =>
            _content?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?
                .FirstOrDefault()?["itemSectionRenderer"]?["contents"]?
                .Values<JObject>() ?? new List<JObject>()
        );

        public string? TryGetContinuation() => _TryGetContinuation();

        private string? _TryGetContinuation(JObject? content = null) => Memo.Cache(this, () =>
            content?["continuationItemRenderer"]?["continuationEndpoint"]?["continuationCommand"]?["token"]?.Value<string>()
        );

        public IEnumerable<JToken> GetCommunityItems()
        {
            var contents = GetCommunityContents();
            foreach (var content in contents)
            {
                if (content?.ContainsKey("backstagePostThreadRenderer") == true)
                {
                    var community = content["backstagePostThreadRenderer"]?["post"]?["backstagePostRenderer"];
                    if (community != null)
                    {
                        yield return community;
                    }
                    continue;
                }
                _TryGetContinuation(content);
            }
        }

        // For GetNextCommunitysList
        private IEnumerable<JObject?> GetCommunityContentsFromNext() => Memo.Cache(this, () =>
            _content?["onResponseReceivedEndpoints"]?
                .FirstOrDefault()?["appendContinuationItemsAction"]?["continuationItems"]?
                .Values<JObject>() ?? new List<JObject>()
        );
        public IEnumerable<JToken> GetCommunityItemsFromNext()
        {
            var contents = GetCommunityContentsFromNext();
            foreach (var content in contents)
            {
                if (content?.ContainsKey("backstagePostThreadRenderer") == true)
                {
                    var community = content["backstagePostThreadRenderer"]?["post"]?["backstagePostRenderer"];
                    if (community != null)
                    {
                        yield return community;
                    }
                    continue;
                }
                _TryGetContinuation(content);
            }
        }
    }
}
