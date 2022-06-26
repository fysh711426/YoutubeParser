using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.Extensions;

namespace YoutubeParser
{
    public abstract class YoutubeParserBase
    {
        protected static readonly string hl = "en";
        //protected static readonly string gl = "US";
        protected static readonly string apiKey = "AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8";
        protected static readonly string userAgent =
            @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36";
        protected readonly HttpClient _httpClient;

        public YoutubeParserBase(HttpClient httpClient)
        {
            _httpClient = httpClient;
            if (_httpClient.DefaultRequestHeaders.ConnectionClose == null)
                _httpClient.DefaultRequestHeaders.ConnectionClose = true;
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        }

        public string GetChannelUrl(string urlOrChannelId)
        {
            if (!urlOrChannelId.Contains("www.youtube.com"))
                return $"https://www.youtube.com/channel/{urlOrChannelId}";
            return urlOrChannelId;
        }
    }
}
