using YoutubeParser;
using YoutubeParser.Models;

namespace Example
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var channel = new YoutubeChannel("channelId");
            var info = await channel.GetInfoAsync();
            var videos = await channel.GetVideosAsync();
            while(true)
            {
                var nextVideos = await channel.GetNextVideosAsync();
                if (nextVideos == null)
                    break;
                videos.AddRange(nextVideos);
            }
        }
    }
}