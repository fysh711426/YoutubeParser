using YoutubeParser;
using YoutubeParser.Models;

namespace Example
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var channel = new YoutubeChannel("UrlOrChannelId");

            // Get About
            var about = await channel.GetAboutAsync();

            // Get Videos
            var videoList = new List<Video>();
            var enumerable = channel.GetVideosAsync();
            await foreach (var item in enumerable)
            {
                videoList.Add(item);
            }

            // NET45 or NET46
            var videos = await channel.GetVideosListAsync();
            while (true)
            {
                var nextVideos = await channel.GetNextVideosListAsync();
                if (nextVideos == null)
                    break;
                videos.AddRange(nextVideos);
            }
        }
    }
}