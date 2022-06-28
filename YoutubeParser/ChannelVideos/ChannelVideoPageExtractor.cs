using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeParser.Utils;

namespace YoutubeParser.ChannelVideos
{
    internal class ChannelVideoPageExtractor
    {
        private readonly JObject? _content;

        public ChannelVideoPageExtractor(JObject? content) => _content = content;

        private JToken? TryGetTabs() => Memo.Cache(this, () =>
            _content?["contents"]?["twoColumnBrowseResultsRenderer"]?["tabs"]
        );

        public JObject? TryGetSelectedTab() => Memo.Cache(this, () =>
            TryGetTabs()?.Values<JObject>()
                .Where(it => it?["tabRenderer"]?["selected"]?.Value<bool>() == true)
                .FirstOrDefault()
        );

        private IEnumerable<JObject?> GetVideoGrids() => Memo.Cache(this, () =>
            TryGetSelectedTab()?["tabRenderer"]?["content"]?["sectionListRenderer"]?["contents"]?
                .FirstOrDefault()?["itemSectionRenderer"]?["contents"]?
                .FirstOrDefault()?["gridRenderer"]?["items"]?
                .Values<JObject>() ?? new List<JObject>()
        );

        public string? TryGetContinuation() => _TryGetContinuation();

        private string? _TryGetContinuation(JObject? grid = null) => Memo.Cache(this, () =>
            grid?["continuationItemRenderer"]?["continuationEndpoint"]?["continuationCommand"]?["token"]?.Value<string>()
        );

        public IEnumerable<JToken> GetVideoItems()
        {
            var grids = GetVideoGrids();
            foreach (var grid in grids)
            {
                if (grid?.ContainsKey("gridVideoRenderer") == true)
                {
                    var gridVideo = grid["gridVideoRenderer"];
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
            _content?["onResponseReceivedActions"]?
                .FirstOrDefault()?["appendContinuationItemsAction"]?["continuationItems"]?
                .Values<JObject>() ?? new List<JObject>()
        );
        public IEnumerable<JToken> GetVideoItemsFromNext()
        {
            var grids = GetVideoGridsFromNext();
            foreach (var grid in grids)
            {
                if (grid?.ContainsKey("gridVideoRenderer") == true)
                {
                    var gridVideo = grid["gridVideoRenderer"];
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
