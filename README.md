# YoutubeParser  

This is a library for parsing youtube pages.  

It focus more on channel than other libraries, for example you can use it to get channel videos, live videos, upcoming videos, or community articles.  

---  

### Nuget install  

```
PM> Install-Package YoutubeParser
```  

---  

### IAsyncEnumerable  

```
PM> Install-Package System.Linq.Async
```  

> If you want to use LINQ on IAsyncEnumerable you need to install this package.  

---  

### Example  

`videoId` or `channelId` can use url or id.  

```C#
var youtube = new YoutubeClient();

// Get Video
var video = await youtube.Video.GetAsync(videoId);

// Get Channel
var channel = await youtube.Channel.GetAsync(channelId);

// Get Community
var community = await youtube.Community.GetAsync(communityId);
```

---  

#### Get Channel Videos  

```C#
var videos = await youtube.Channel
    .GetVideosAsync(channelId)
    .ToListAsync();
```

Or use await foreach.  

```C#
var videos = new List<ChannelVideo>();
var enumerable = youtube.Channel.GetVideosAsync(channelId);
await foreach (var item in enumerable)
{
    videos.Add(item);
}
```

`NET45` or `NET46` you can only use this.  

```C#
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
```

---  

#### Get Live Streams Videos  

```C#
var liveStreams = await youtube.Channel
    .GetLiveAsync(channelId)
    .ToListAsync();
```

#### Get Upcoming Live Streams Videos  

```C#
var upcomingLiveStreams = await youtube.Channel
    .GetUpcomingLiveAsync(channelId)
    .ToListAsync();
```

---  

#### Get Channel Communitys  

```C#
var communitys = await youtube.Channel
    .GetCommunitysAsync(channelId)
    .ToListAsync();
```

---  

#### Get Video Comments  

```C#
var videoComments = await youtube.Video
    .GetCommentsAsync(videoId)
    .ToListAsync();
```

#### Get Community Comments  

```C#
var communityComments = await youtube.Community
    .GetCommentsAsync(communityId)
    .ToListAsync();
```

#### Get Comment Replies  

```C#
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
```

---  

#### Get Video TopChats  

```C#
var topChats = await youtube.Video
    .GetTopChatsAsync(videoId)
    .ToListAsync();
```

#### Get Video LiveChats  

```C#
var liveChats = await youtube.Video
    .GetLiveChatsAsync(videoId)
    .ToListAsync();
```

---  

### Parameters  

* You can use parameters to filter video list.  

#### Get all past live streams  

```C#
var pastLiveStreams = await youtube.Channel
    .GetVideosAsync(channelId)
    .Where(it =>
        it.VideoType == VideoType.Stream &&
        it.VideoStatus == VideoStatus.Default)
    .ToListAsync();
```

* VideoType 

 Type    | Description 
---------|-------------------
 Video   | Upload video
 Stream  | Live stream video

* VideoStatus  

 Status   | Description
----------|------------------
 Default  | Streamed or Premiered or Upload video
 Live     | Streaming or Premiering
 Upcoming | Scheduled or Premieres

---  

#### Get shorts videos  

```C#
var shortsVideos = await youtube.Channel
    .GetVideosAsync(channelId)
    .Where(it => it.IsShorts)
    .ToListAsync();
```

---  

#### Get videos in last month  

```C#
var inLastMonth = await youtube.Channel
    .GetVideosAsync(channelId)
    .BreakOn(it => it.PublishedTimeSeconds >= TimeSeconds.Month)
    .ToListAsync();
```

The `BreakOn` method leaves loop when the condition is true, use it can prevent requests wasted.  

```C#
public static async IAsyncEnumerable<T> BreakOn<T>(
    this IAsyncEnumerable<T> source, Func<T, bool> predicate)
{
    await foreach(var item in source)
    {
        if (predicate(item))
            break;
        yield return item;
    }
}
```

---  

#### Get super thanks  

```C#
var superThanks = await youtube.Video
    .GetCommentsAsync(videoId)
    .Where(it => it.CommentType == CommentType.SuperThanks)
    .ToListAsync();
```

---  

#### Get super chats  

```C#
var superChats = await youtube.Video
    .GetTopChatsAsync(videoId)
    .Where(it => it.LiveChatType == LiveChatType.SuperChat)
    .ToListAsync();
```

#### Get gift chats

```C#
var giftChats = await youtube.Video
    .GetTopChatsAsync(videoId)
    .Where(it => it.LiveChatType == LiveChatType.Gift)
    .ToListAsync();
```

* LiveChatType  

 Type       | Description
------------|------------------
 Text       | Text message
 SuperChat  | SuperChat or SuperSticker
 Gift       | Give member gift or Receive member gift
 Membership | Member join or Member chat
