using System;
using System.Linq;
using System.Net.Http;

namespace YoutubeParser.Shares
{
    public abstract class YoutubeParserBase
    {
        protected static readonly string hl = "en";
        //protected static readonly string gl = "US";
        protected static readonly string apiKey = "AIzaSyAO_FJ2SlqU8Q4STEHLGCilw_Y9_11qcW8";
        protected static readonly string userAgent =
            @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36";
        protected readonly HttpClient _httpClient;
        protected readonly Func<int>? _requestDelay;

        protected YoutubeParserBase(
            HttpClient httpClient, Func<int>? requestDelay)
        {
            _httpClient = httpClient;
            _requestDelay = requestDelay;
        }

        protected string GetChannelUrl(string urlOrChannelId)
        {
            if (!urlOrChannelId.Contains("www.youtube.com"))
                return $"https://www.youtube.com/channel/{urlOrChannelId}";
            return urlOrChannelId;
        }

        protected string GetVideoUrl(string urlOrVideolId)
        {
            if (!urlOrVideolId.Contains("www.youtube.com"))
                return $"https://www.youtube.com/watch?v={urlOrVideolId}";
            return urlOrVideolId;
        }

        protected string GetCommunityUrl(string urlOrCommunityId)
        {
            if (!urlOrCommunityId.Contains("www.youtube.com"))
                return $"https://www.youtube.com/post/{urlOrCommunityId}";
            return urlOrCommunityId;
        }

        protected void SetDefaultHttpRequest(HttpRequestMessage request)
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
