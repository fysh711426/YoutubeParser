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
        }

        public string GetChannelUrl(string urlOrChannelId)
        {
            if (!urlOrChannelId.Contains("www.youtube.com"))
                return $"https://www.youtube.com/channel/{urlOrChannelId}";
            return urlOrChannelId;
        }

        public string GetVideoUrl(string urlOrVideolId)
        {
            if (!urlOrVideolId.Contains("www.youtube.com"))
                return $"https://www.youtube.com/watch?v={urlOrVideolId}";
            return urlOrVideolId;
        }

        public void SetDefaultHttpRequest(HttpRequestMessage request)
        {
            if (request.Headers.ConnectionClose == null)
                request.Headers.ConnectionClose = true;
            if (!request.Headers.Contains("User-Agent"))
                request.Headers.Add("User-Agent", userAgent);
            var cookie = request.Headers.TryGetValues("Cookie", out var cookies)
                ? cookies.FirstOrDefault()?.Trim() ?? "" : "";
            cookie += cookie != "" && !cookie.EndsWith(";") ? "; " : "";
            cookie += $"PREF=hl={hl}";
            //cookie += $"; CONSENT=YES+cb; YSC=DwKYllHNwuw";
            request.Headers.Add("Cookie", cookie);
        }
    }
}
