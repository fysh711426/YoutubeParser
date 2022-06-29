using Newtonsoft.Json;
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

namespace YoutubeParser.Commons
{
    internal class YoutubePageExtractor
    {
        private readonly string? _html;

        public YoutubePageExtractor(string? html) => _html = html;

        public JObject? TryGetInitialData() => Memo.Cache(this, () =>
            _html?
                .Pipe(it => Regex.Matches(it, @"<script.*?>([\s\S]*?)<\/script>"))
                .SelectMany(m => m.Groups[1].Value)
                .Pipes(it => Regex.Match(it, @"var ytInitialData = (\{.*\})"))
                .Pipes(it => it.Groups[1].Value)
                .FirstOrDefault(it => !string.IsNullOrEmpty(it))?
                .Pipe(it => JsonConvert.DeserializeObject<JObject>(it))
        );

        public JToken? TryGetTabs() => Memo.Cache(this, () =>
            TryGetInitialData()?["contents"]?["twoColumnBrowseResultsRenderer"]?["tabs"]
        );

        public JObject? TryGetSelectedTab() => Memo.Cache(this, () =>
            TryGetTabs()?.Values<JObject>()
                .Where(it => it?["tabRenderer"]?["selected"]?.Value<bool>() == true)
                .FirstOrDefault()
        );

        public JObject? TryGetInitialPlayerResponse() => Memo.Cache(this, () =>
            _html?
                .Pipe(it => Regex.Matches(it, @"<script.*?>([\s\S]*?)<\/script>"))
                .SelectMany(m => m.Groups[1].Value)
                .Pipes(it => Regex.Match(it, @"var ytInitialPlayerResponse = (\{.*\})"))
                .Pipes(it => it.Groups[1].Value)
                .FirstOrDefault(it => !string.IsNullOrEmpty(it))?
                .Pipe(it => JsonConvert.DeserializeObject<JObject>(it))
        );

        public JObject? TryGetJsonResponse() => Memo.Cache(this, () =>
            _html != null ? JsonConvert.DeserializeObject<JObject>(_html) : null
        );
    }
}
