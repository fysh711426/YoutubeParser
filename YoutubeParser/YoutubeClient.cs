using System.Net.Http;
using YoutubeParser.Channels;
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
        public YoutubeClient() : 
            this(Http.Client)
        {
        }
        public YoutubeClient(HttpClient httpClient)
        {
            Channel = new YoutubeChannelParser(httpClient);
            Video = new YoutubeVideoParser(httpClient);
            Community = new YoutubeCommunityParser(httpClient);
        }
    }
}
