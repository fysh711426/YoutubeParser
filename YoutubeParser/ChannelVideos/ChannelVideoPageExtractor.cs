using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using YoutubeParser.Shares;
using YoutubeParser.Utils;

namespace YoutubeParser.ChannelVideos
{
    internal class ChannelVideoPageExtractor
    {
        private readonly string? _html;

        public ChannelVideoPageExtractor(string? html) => _html = html;
        
        public JObject? TryGetSelectedTab() => Memo.Cache(this, () =>
            new YoutubePageExtractor(_html).TryGetSelectedTab()
        );

        public string GetSelectedSubMenuTitle() => Memo.Cache(this, () =>
            new YoutubePageExtractor(_html).TryGetSelectedSubMenu()?["title"]?.Value<string>() ?? ""
        );

        private IEnumerable<JObject?> GetVideoGrids() => Memo.Cache(this, () =>
            TryGetSelectedTab()?["tabRenderer"]?["content"]?["richGridRenderer"]?["contents"]?
                .Values<JObject>() ?? new List<JObject>()
        );

        public JToken? TryGetInnerTubeContext() =>
            new YoutubePageExtractor(_html)
                .TryGetYtcfg()?["INNERTUBE_CONTEXT"];

        public string? TryGetContinuation() => _TryGetContinuation();

        private string? _TryGetContinuation(JObject? grid = null) => Memo.Cache(this, () =>
            grid?["continuationItemRenderer"]?["continuationEndpoint"]?["continuationCommand"]?["token"]?.Value<string>()
        );

        public IEnumerable<JToken> GetVideoItems()
        {
            var grids = GetVideoGrids();
            foreach (var grid in grids)
            {
                if (grid?.ContainsKey("richItemRenderer") == true)
                {
                    var gridVideo =
                        // Video and Stream
                        grid["richItemRenderer"]?["content"]?["videoRenderer"] ??
                        // Shorts
                        grid["richItemRenderer"]?["content"]?["reelItemRenderer"];
                    if (gridVideo != null)
                    {
                        yield return gridVideo;
                    }
                    continue;
                }
                _TryGetContinuation(grid);
            }
        }

        // For GetNextVideosList
        private IEnumerable<JObject?> GetVideoGridsFromNext() => Memo.Cache(this, () =>
            new YoutubePageExtractor(_html)
                .TryGetJsonResponse()?["onResponseReceivedActions"]?
                    .FirstOrDefault()?["appendContinuationItemsAction"]?["continuationItems"]?
                        .Values<JObject>() ?? new List<JObject>()
        );

        public IEnumerable<JToken> GetVideoItemsFromNext()
        {
            var grids = GetVideoGridsFromNext();
            foreach (var grid in grids)
            {
                if (grid?.ContainsKey("richItemRenderer") == true)
                {
                    var gridVideo = 
                        // Video and Stream
                        grid["richItemRenderer"]?["content"]?["videoRenderer"] ??
                        // Shorts
                        grid["richItemRenderer"]?["content"]?["reelItemRenderer"];
                    if (gridVideo != null)
                    {
                        yield return gridVideo;
                    }
                    continue;
                }
                _TryGetContinuation(grid);
            }
        }
    }
}
