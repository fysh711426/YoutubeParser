using System.Globalization;
using YoutubeParser;
using YoutubeParser.Channels;

namespace Example
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var channelUrl = "UrlOrChannelId";

            var youtube = new YoutubeClient();

            // Get Channel
            var channel = await youtube.Channel.GetAsync(channelUrl);

            // Get Channel Videos
            var videoList = new List<ChannelVideo>();
            var enumerable = youtube.Channel.GetVideosAsync(channelUrl);
            await foreach (var item in enumerable)
            {
                var x = item.ViewCount;
                videoList.Add(item);
            }

            // NET45 or NET46
            var videos = await youtube.Channel.GetVideosListAsync(channelUrl);
            while (true)
            {
                var nextVideos = await youtube.Channel.GetNextVideosListAsync();
                if (nextVideos == null)
                    break;
                videos.AddRange(nextVideos);
            }
        }
    }
}