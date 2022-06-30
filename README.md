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
```

#### Get Live Streams Videos  

```C#
// Get Live Streams Videos
var liveList = new List<ChannelVideo>();
var liveStreams = youtube.Channel.GetLiveAsync(channelId);
await foreach(var item in liveStreams)
{
    liveList.Add(item);
}
```

#### Get Upcoming Live Streams Videos  

```C#
// Get Upcoming Live Streams Videos
var upcomingLiveList = new List<ChannelVideo>();
var upcomingLiveStreams = youtube.Channel.GetUpcomingLiveAsync(channelId);
await foreach (var item in upcomingLiveStreams)
{
    upcomingLiveList.Add(item);
}
```

#### Get Channel Communitys  

```C#
// Get Channel Communitys
var communityList = new List<Community>();
var communityEnumerable = youtube.Channel.GetCommunitysAsync(channelId);
await foreach (var item in communityEnumerable)
{
    communityList.Add(item);
}
```

---  

### Parameters  

You can use these two parameters to filter videos in the video list.  

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
 