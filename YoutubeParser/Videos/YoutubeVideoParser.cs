using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.Extensions;

namespace YoutubeParser.Videos
{
    public class YoutubeVideoParser : YoutubeParserBase
    {
        public YoutubeVideoParser(HttpClient httpClient)
            : base(httpClient)
        {
        }

        // ----- GetVideo -----
        public async Task<Video> GetAsync(string urlOrVideolId)
        {
            var url = $"{GetVideoUrl(urlOrVideolId)}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var extractor = new VideoPageExtractor(html);

            var video = new Video
            {
                VideoId = extractor.GetVideoId(),
                Title = extractor.GetTitle(),
                Description = extractor.GetDescription(),
                AuthorTitle = extractor.GetAuthorTitle(),
                AuthorChannelId = extractor.GetAuthorChannelId(),
                Duration = extractor.TryGetDuration(),
                Thumbnails = extractor.GetThumbnails(),
                Keywords = extractor.GetKeywords(),
                ViewCount = extractor.GetViewCount(),
                UploadDate = extractor.GetUploadDate(),
                LikeCount = extractor.TryGetLikeCount(),
                IsPrivate = extractor.IsPrivate(),
                IsPlayable = extractor.IsPlayable(),
                VideoType = extractor.GetVideoType(),
                VideoStatus = extractor.GetVideoStatus()
            };
            return video;
        }
        // ----- GetVideo -----
    }
}
