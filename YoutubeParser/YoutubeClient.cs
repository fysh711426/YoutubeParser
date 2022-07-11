using System;
using System.Net.Http;
using YoutubeParser.Channels;
using YoutubeParser.Comments;
using YoutubeParser.Communitys;
using YoutubeParser.Utils;
using YoutubeParser.Videos;

namespace YoutubeParser
{
    public class YoutubeClient
    {
        public YoutubeChannelParser Channel { get; set; }
        public YoutubeVideoParser Video { get; set; }
        public YoutubeCommunityParser Community { get; set; }
        public YoutubeCommentParser Comment { get; set; }

        public YoutubeClient() : 
            this(Http.Client, null)
        {
        }

        public YoutubeClient(HttpClient httpClient) :
            this(httpClient, null)
        {
        }

        public YoutubeClient(Func<int> requestDelay) :
            this(Http.Client, requestDelay)
        {
        }

        public YoutubeClient(HttpClient httpClient, Func<int>? requestDelay)
        {
            Channel = new YoutubeChannelParser(httpClient, requestDelay);
            Video = new YoutubeVideoParser(httpClient, requestDelay);
            Community = new YoutubeCommunityParser(httpClient, requestDelay);
            Comment = new YoutubeCommentParser(httpClient, requestDelay);
        }
    }
}
