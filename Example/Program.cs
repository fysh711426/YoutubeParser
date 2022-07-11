﻿using System.Text;
using YoutubeParser;
using YoutubeParser.Extensions;
using YoutubeParser.Shares;
using YoutubeParser.Utils;

namespace Example
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var videoId = "VideoUrl or VideoId";
            var channelId = "ChannelUrl or ChannelId";
            var communityId = "CommunityUrl or CommunityId";

            // Each request in client is delay by 1 second
            var youtube = new YoutubeClient(() => 1000);

            // Get Video
            var video = await youtube.Video.GetAsync(videoId);

            // Get Channel
            var channel = await youtube.Channel.GetAsync(channelId);

            // Get Community
            var community = await youtube.Community.GetAsync(communityId);

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
            var liveStreams = await youtube.Channel
                .GetLiveAsync(channelId)
                .ToListAsync();

            // Get Upcoming Live Streams Videos
            var upcomingLiveStreams = await youtube.Channel
                .GetUpcomingLiveAsync(channelId)
                .ToListAsync();

            // Get Channel Communitys
            var communitys = await youtube.Channel
                .GetCommunitysAsync(channelId)
                .ToListAsync();

            // Get Video Comments
            var videoComments = await youtube.Video
                .GetCommentsAsync(videoId)
                .ToListAsync();

            // Get Community Comments
            var communityComments = await youtube.Community
                .GetCommentsAsync(communityId)
                .ToListAsync();

            // Get Comment Replies
            var comments = await youtube.Video
                .GetCommentsAsync(videoId)
                .ToListAsync();
            foreach (var comment in comments)
            {
                if (comment.ReplyCount > 0)
                {
                    comment.Replies = await youtube.Comment
                        .GetRepliesAsync(comment)
                        .ToListAsync();
                }
            }

            // Get Video TopChats
            var topChats = await youtube.Video
                .GetTopChatsAsync(videoId)
                .ToListAsync();

            // Get Video LiveChats
            var liveChats = await youtube.Video
                .GetLiveChatsAsync(videoId)
                .ToListAsync();

            // Receive Video TopChats
            await youtube.Video.OnTopChatsAsync(videoId, (item) =>
            {
                // do something
            });

            // Receive Video LiveChats
            await youtube.Video.OnLiveChatsAsync(videoId, (item) =>
            {
                // do something
            });

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

            // Get videos in last month
            var inLastMonth = await youtube.Channel
                .GetVideosAsync(channelId)
                .BreakOn(it => it.PublishedTimeSeconds >= TimeSeconds.Month)
                .ToListAsync();

            // Get super thanks
            var superThanks = await youtube.Video
                .GetCommentsAsync(videoId)
                .Where(it => it.CommentType == CommentType.SuperThanks)
                .ToListAsync();
            
            // Get super chats
            var superChats = await youtube.Video
                .GetTopChatsAsync(videoId)
                .Where(it => it.LiveChatType == LiveChatType.SuperChat)
                .ToListAsync();

            // Get gift chats
            var giftChats = await youtube.Video
                .GetTopChatsAsync(videoId)
                .Where(it => it.LiveChatType == LiveChatType.Gift)
                .ToListAsync();
        }
    }
}
