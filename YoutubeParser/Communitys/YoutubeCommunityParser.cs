﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeParser.ChannelVideos;
using YoutubeParser.Commons;
using YoutubeParser.Extensions;
using YoutubeParser.Utils;

namespace YoutubeParser.Communitys
{
    public partial class YoutubeCommunityParser : YoutubeParserBase
    {
        public YoutubeCommunityParser(HttpClient httpClient)
            : base(httpClient)
        {
        }

        // ----- GetCommunity -----
        public async Task<Community> GetAsync(string urlOrCommunityId)
        {
            var url = $"{GetCommunityUrl(urlOrCommunityId)}";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            SetDefaultHttpRequest(request);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();
            var pageExtractor = new CommunityPageExtractor(html);
            var item = pageExtractor.GetCommunityItems().FirstOrDefault();
            if (item == null)
                throw new Exception("Not found community.");
            var extractor = new CommunityExtractor(item);
            var community = new Community
            {
                PostId = extractor.GetPostId(),
                AuthorTitle = extractor.GetAuthorTitle(),
                AuthorChannelId = extractor.GetAuthorChannelId(),
                AuthorThumbnails = extractor.GetAuthorThumbnails(),
                Content = extractor.GetContent(),
                Images = extractor.GetImages(),
                PublishedTime = extractor.GetPublishedTime(),
                PublishedTimeSeconds = extractor.GetPublishedTimeSeconds(),
                LikeCount = extractor.GetLikeCount(),
                VoteStatus = extractor.GetVoteStatus(),
                PollStatus = extractor.GetPollStatus()
            };
            return community;
        }
        // ----- GetCommunity -----
    }
}
