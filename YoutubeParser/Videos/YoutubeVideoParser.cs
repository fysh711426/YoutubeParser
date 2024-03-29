﻿using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeParser.Shares;

namespace YoutubeParser.Videos
{
    public partial class YoutubeVideoParser : YoutubeParserBase
    {
        public YoutubeVideoParser(
            HttpClient httpClient, Func<int>? requestDelay)
                : base(httpClient, requestDelay)
        {
        }

        // ----- GetVideo -----

        /// <summary>
        /// Get video info by video url or id.
        /// </summary>
        /// <param name="urlOrVideoId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Video> GetAsync(string urlOrVideoId, CancellationToken token = default)
        {
            var url = $"{GetVideoUrl(urlOrVideoId)}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, 
                HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            token.ThrowIfCancellationRequested();
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
