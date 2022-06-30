using System.Globalization;
using YoutubeParser;
using YoutubeParser.ChannelVideos;

namespace Example
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var videoId = "VideoUrl or VideoId";
            var channelId = "ChannelUrl or ChannelId";

            var youtube = new YoutubeClient();

            // Get Video
            var video = await youtube.Video.GetAsync(videoId);

            // Get Channel
            var channel = await youtube.Channel.GetAsync(channelId);

            // Get Channel Videos
            var videoList = new List<ChannelVideo>();
            var enumerable = youtube.Channel.GetVideosAsync(channelId);
            await foreach (var item in enumerable)
            {
                videoList.Add(item);
            }

            // NET45 or NET46
            var videos = await youtube
                .Channel.GetVideosListAsync(channelId);
            while (true)
            {
                var nextVideos = await youtube
                    .Channel.GetNextVideosListAsync();
                if (nextVideos == null)
                    break;
                videos.AddRange(nextVideos);
            }

            // Get Live Streams Videos
            var liveList = new List<ChannelVideo>();
            var liveStreams = youtube.Channel.GetLiveAsync(channelId);
            await foreach(var item in liveStreams)
            {
                liveList.Add(item);
            }

            // Get Upcoming Live Streams Videos
            var upcomingLiveList = new List<ChannelVideo>();
            var upcomingLiveStreams = youtube.Channel.GetUpcomingLiveAsync(channelId);
            await foreach (var item in upcomingLiveStreams)
            {
                upcomingLiveList.Add(item);
            }

            // Get Channel Communitys
            var communityList = new List<Community>();
            var communityEnumerable = youtube.Channel.GetCommunitysAsync(channelId);
            await foreach (var item in communityEnumerable)
            {
                communityList.Add(item);
            }
        }
    }
}
