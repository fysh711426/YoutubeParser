using System.Globalization;
using YoutubeParser;
using YoutubeParser.ChannelVideos;
using YoutubeParser.Commons;
using YoutubeParser.Extensions;
using YoutubeParser.Utils;

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

            // NET45 or NET46
            var videoList = await youtube
                .Channel.GetVideosListAsync(channelId);
            while (true)
            {
                var nextVideoList = await youtube
                    .Channel.GetNextVideosListAsync();
                if (nextVideoList == null)
                    break;
                videoList.AddRange(nextVideoList);
            }

            // Get Channel Videos
            var videos = await youtube.Channel
                .GetVideosAsync(channelId)
                .ToListAsync();

            // Get Live Streams Videos
            var liveStreams = youtube.Channel
                .GetLiveAsync(channelId)
                .ToListAsync();

            // Get Upcoming Live Streams Videos
            var upcomingLiveStreams = youtube.Channel
                .GetUpcomingLiveAsync(channelId)
                .ToListAsync();

            // Get Channel Communitys
            var communitys = youtube.Channel
                .GetCommunitysAsync(channelId)
                .ToListAsync();

            // Get all past live streams
            var pastLiveStreams = await youtube.Channel
                .GetVideosAsync(channelId)
                .Where(it =>
                    it.VideoType == VideoType.Stream &&
                    it.VideoStatus == VideoStatus.Default)
                .ToListAsync();

            // Get shorts videos
            var shortsVideos = await youtube.Channel
                .GetVideosAsync(channelId)
                .Where(it => it.IsShorts)
                .ToListAsync();

            // Get Videos in last month
            var inLastMonth = await youtube.Channel
                .GetVideosAsync(channelId)
                .Break(it => it.PublishedTimeSeconds >= TimeSeconds.Month)
                .ToListAsync();
        }
    }
}
