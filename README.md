# YoutubeParser  

This is a library for parsing youtube pages.  

It focuses more on channel pages than others, for example you can use it to get channel videos, live videos, upcoming videos, or community articles.  

The library uses web pages to fetch data, as it doesn't rely on the official API, there's also no need for an API key.  

---  

### Nuget install  

```
PM> Install-Package YoutubeParser
```  

---  

### Example  

`videoId` or `channelId` can use url or id.  

```C#
var youtube = new YoutubeClient();

// Get Video
var video = await youtube.Video.GetAsync(videoId);

// Get Channel
var channel = await youtube.Channel.GetAsync(channelId);
```

#### Get Channel Videos  

```C#
// Get Channel Videos
var videos = new List<ChannelVideo>();
var enumerable = youtube.Channel.GetVideosAsync(channelId);
await foreach (var item in enumerable)
{
    videos.Add(item);
}

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
```

> If you want to use LINQ on IAsyncEnumerable you need to install this package.  

```
PM> Install-Package System.Linq.Async
```  

```C#
var videos = await youtube.Channel
    .GetVideosAsync(channelId)
    .ToListAsync();
```

#### Get Live Streams Videos  

```C#
var liveStreams = youtube.Channel
    .GetLiveAsync(channelId)
    .ToListAsync();
```

#### Get Upcoming Live Streams Videos  

```C#
var upcomingLiveStreams = youtube.Channel
    .GetUpcomingLiveAsync(channelId)
    .ToListAsync();
```

#### Get Channel Communitys  

```C#
var communitys = youtube.Channel
    .GetCommunitysAsync(channelId)
    .ToListAsync();
```

---  

### Parameters  

You can use parameters to filter videos in the video list.  

* VideoType 

 Type    | Description 
---------|-------------------
 Video   | Upload video
 Stream  | Live stream video

* VideoStatus  

Status    | Description
----------|------------------
 Default  | Streamed or Premiered or Upload video
 Live     | Streaming or Premiere in progress
 Upcoming | Scheduled or Premieres

#### Get all past live streams  

```C#
var pastLiveStreams = await youtube.Channel
    .GetVideosAsync(channelId)
    .Where(it =>
        it.VideoType == VideoType.Stream &&
        it.VideoStatus == VideoStatus.Default)
    .ToListAsync();
```

#### Get shorts videos  

```C#
var shortsVideos = await youtube.Channel
    .GetVideosAsync(channelId)
    .Where(it => it.IsShorts)
    .ToListAsync();
```

#### Get videos in last 30 days  

```C#
var inLast30Days = await youtube.Channel
    .GetVideosAsync(channelId)
    .Break(it => it.PublishedTimeSeconds >= TimeSeconds.Month)
    .ToListAsync();
```

The `Break` method leaves loop when the condition is true, use it can prevent requests wasted.  

```C#
await foreach(var item in source)
{
    if (predicate(item))
        break;
    yield return item;
}
```