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
// Get channel videos by url or channelId
var videos = await youtube.Channel
    .GetVideosAsync(channelId)
    .ToListAsync();
```

#### Get Channel Streams  

```C#
// Get channel stream videos by url or channelId
var streams = await youtube.Channel
    .GetStreamsAsync(channelId)
    .ToListAsync();
```

#### Get Channel Shorts  

```C#
// Get channel short videos by url or channelId
var shorts = await youtube.Channel
    .GetShortsAsync(channelId)
    .ToListAsync();
```

#### Get Channel Communitys  

```C#
// Get channel communitys by url or channelId
var communitys = await youtube.Channel
    .GetCommunitysAsync(channelId)
    .ToListAsync();
```

---  

#### Get Video Comments  

```C#
// Get video comments by url or videoId
var videoComments = await youtube.Video
    .GetCommentsAsync(videoId)
    .ToListAsync();
```

#### Get Video Comment Replies  

```C#
// Get video comment replies by comment
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

---  

#### Get Community Comments  

```C#
// Get community comments by url or communityId
var communityComments = await youtube.Community
    .GetCommentsAsync(communityId)
    .ToListAsync();
```

#### Get Community Comment Replies  

```C#
// Get community comment replies by comment
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
// Get stream video top chats by url or videoId
var topChats = await youtube.Video
    .GetTopChatsAsync(videoId)
    .ToListAsync();
```

#### Get Video LiveChats  

```C#
// Get stream video live chats by url or videoId
var liveChats = await youtube.Video
    .GetLiveChatsAsync(videoId)
    .ToListAsync();
```

---  

> **Note**  
> If you want to receive live chat instantly, you can use it.  

#### Receive Video TopChats  

```C#
// Receive stream video top chats by url or videoId
await youtube.Video.OnTopChatsAsync(videoId, (item) =>
{
    // do something
});
```

#### Receive Video LiveChats  

```C#
// Receive stream video live chats by url or videoId
await youtube.Video.OnLiveChatsAsync(videoId, (item) =>
{
    // do something
});
```

---  

#### Get live stream  

```C#
// Get live stream video by url or channelId
var liveStream = await youtube.Channel
    .GetStreamsAsync(channelId)
    .FirstOrDefaultAsync(it => 
        it.VideoStatus == VideoStatus.Live);
```

#### Get videos in last month  

```C#
// Get channel videos in last month by url or channelId
var inLastMonth = await youtube.Channel
    .GetVideosAsync(channelId)
    .BreakOn(it => it.PublishedTimeSeconds >= TimeSeconds.Month)
    .ToListAsync();
```

#### Get super thanks  

```C#
// Get video super thanks by url or videoId
var superThanks = await youtube.Video
    .GetCommentsAsync(videoId)
    .Where(it => it.CommentType == CommentType.SuperThanks)
    .ToListAsync();
```

#### Get super chats  

```C#
// Get stream video super chats by url or videoId
var superChats = await youtube.Video
    .GetLiveChatsAsync(videoId)
    .Where(it => it.LiveChatType == LiveChatType.SuperChat)
    .ToListAsync();
```

#### Get gift chats

```C#
// Get stream video gift chats by url or videoId
var giftChats = await youtube.Video
    .GetLiveChatsAsync(videoId)
    .Where(it => it.LiveChatType == LiveChatType.Gift)
    .ToListAsync();
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

* LiveChatType  

 Type       | Description
------------|------------------
 Text       | Text message
 SuperChat  | SuperChat or SuperSticker
 Gift       | Give member gift or Receive member gift
 Membership | Member join or Member chat

---  

### BreakOn  

The `BreakOn` method leaves loop when the condition is true, use it can prevent requests wasted.  

```C#
var inLastMonth = await youtube.Channel
    .GetVideosAsync(channelId)
    .BreakOn(it => it.PublishedTimeSeconds >= TimeSeconds.Month)
    .ToListAsync();
```

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

### await foreach  

You can use `await foreach` loop `IAsyncEnumerable`.  

```C#
var videos = new List<ChannelVideo>();
var enumerable = youtube.Channel.GetVideosAsync(channelId);
await foreach (var item in enumerable)
{
    videos.Add(item);
}
```

---  

### NET45 or NET46  

`IAsyncEnumerable` doesn't work in `NET45` and `NET46`, you need to use this instead.  

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
