﻿using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using YoutubeParser.Shares;
using YoutubeParser.Utils;

namespace YoutubeParser.Communitys
{
    internal class CommunityPageExtractor
    {
        private readonly string? _html;

        public CommunityPageExtractor(string? html) => _html = html;

        public JObject? TryGetSelectedTab() => Memo.Cache(this, () =>
            new YoutubePageExtractor(_html).TryGetSelectedTab()
        );

        private IEnumerable<JObject?> GetCommunityContents() => Memo.Cache(this, () =>
            TryGetSelectedTab()?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?
                .FirstOrDefault()?["itemSectionRenderer"]?["contents"]?
                .Values<JObject>() ?? new List<JObject>()
        );

        public JToken? TryGetInnerTubeContext() =>
            new YoutubePageExtractor(_html)
                .TryGetYtcfg()?["INNERTUBE_CONTEXT"];

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
            new YoutubePageExtractor(_html)
                .TryGetJsonResponse()?["onResponseReceivedEndpoints"]?
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
