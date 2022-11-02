# YoutubeParser  

This is a library for parsing youtube pages.  

It focus more on channel than other libraries, for example you can use it to get channel videos, live videos, upcoming videos, community articles, super chats, or super thanks.  

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
// Delay 1 second each request in client
var youtube = new YoutubeClient(() => 1000);

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

#### Get Channel Streams  

```C#
var streams = await youtube.Channel
    .GetStreamsAsync(channelId)
    .ToListAsync();
```

#### Get Channel Shorts  

```C#
var shorts = await youtube.Channel
    .GetShortsAsync(channelId)
    .ToListAsync();
```

---  

#### Get Live Streams Videos [Obsolete]  

```C#
var liveStreams = await youtube.Channel
    .GetLiveAsync(channelId)
    .ToListAsync();
```

#### Get Upcoming Live Streams Videos [Obsolete]  

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

#### Get Video Comment Replies  
```C#
foreach (var comment in videoComments)
{
    if (comment.ReplyCount > 0)
    {
        comment.Replies = await youtube.Comment
            .GetRepliesAsync(comment)
            .ToListAsync();
    }
}
```

#### Get Community Comments  

```C#
var communityComments = await youtube.Community
    .GetCommentsAsync(communityId)
    .ToListAsync();
```

#### Get Community Comment Replies  

```C#
foreach (var comment in communityComments)
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

> **Note**  
> If you want to receive chat continuously, you can use this.  

#### Receive Video TopChats  

```C#
await youtube.Video.OnTopChatsAsync(videoId, (item) =>
{
    // do something
});
```

#### Receive Video LiveChats  

```C#
await youtube.Video.OnLiveChatsAsync(videoId, (item) =>
{
    // do something
});
```

---  

### Parameters  

You can use parameters to filter video list.  

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

#### Get live stream  

```C#
var liveStreams = await youtube.Channel
    .GetStreamsAsync(channelId)
    .BreakOnNext(it => it.VideoStatus == VideoStatus.Live)
    .Where(it => it.VideoStatus == VideoStatus.Live)
    .ToListAsync();
```

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

#### Get super thanks  

```C#
var superThanks = await youtube.Video
    .GetCommentsAsync(videoId)
    .Where(it => it.CommentType == CommentType.SuperThanks)
    .ToListAsync();
```

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
