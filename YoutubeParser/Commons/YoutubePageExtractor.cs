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
        private readonly string _html;

        public YoutubePageExtractor(string? html) => _html = html ?? "";

        public JObject? TryGetInitialData() => Memo.Cache(this, () =>
            _html
                .Pipe(it => Regex.Matches(it, @"<script.*?>([\s\S]*?)<\/script>"))
                .SelectMany(it => it.Groups[1].Value)
                .Where(it => it.Contains("ytInitialData"))
                .FirstOrDefault()?
                .Pipe(it => new StringBuilder(it)
                    //.Replace("var ytInitialData = ", "")
                    .Remove(0, 20)
                    //.Substring(0, it.Length - 1)
                    .Remove(it.Length - 1, 1)
                    .ToString())
                .Pipe(it => JsonConvert.DeserializeObject<JObject>(it))
        );

        private JToken? TryGetTabs() => Memo.Cache(this, () =>
            TryGetInitialData()?["contents"]?["twoColumnBrowseResultsRenderer"]?["tabs"]
        );

        public JObject? TryGetSelectedTab() => Memo.Cache(this, () =>
            TryGetTabs()?.Values<JObject>()
                .Where(it => it?["tabRenderer"]?["selected"]?.Value<bool>() == true)
                .FirstOrDefault()
        );
    }
}
