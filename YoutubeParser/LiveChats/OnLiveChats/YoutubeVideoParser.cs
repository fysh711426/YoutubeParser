using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeParser.LiveChats;

namespace YoutubeParser.Videos
{
    public partial class YoutubeVideoParser
    {
        public async Task OnTopChatsAsync(string urlOrVideoId,
            Action<LiveChat> callback, CancellationToken token = default)
        {
            await OnLiveChatsHandlerAsync(true, urlOrVideoId, callback, token);
        }

        public async Task OnLiveChatsAsync(string urlOrVideoId,
            Action<LiveChat> callback, CancellationToken token = default)
        {
            await OnLiveChatsHandlerAsync(false, urlOrVideoId, callback, token);
        }

        private async Task OnLiveChatsHandlerAsync(bool isTop, string urlOrVideoId,
            Action<LiveChat> callback, CancellationToken token = default)
        {
            var queue = new ConcurrentQueue<(LiveChat liveChat, DateTime timeout)>();
            var video = new YoutubeVideoParser(_httpClient, _requestDelay);
            video._loop = true;

            var delayWarp = async (int delay, CancellationToken token) =>
            {
                try
                {
                    await Task.Delay(delay, token);
                }
                catch (TaskCanceledException) { }
            };

            var isRun = false;
            var runTask = () =>
            {
                if (!isRun)
                {
                    isRun = true;
                    Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (queue.IsEmpty || token.IsCancellationRequested)
                            {
                                isRun = false;
                                break;
                            }
                            if (queue.TryDequeue(out var item))
                            {
                                var delay = (int)(item.timeout - DateTime.UtcNow).TotalMilliseconds;
                                if (delay > 0)
                                    await delayWarp(delay, token);
                                callback(item.liveChat);
                            }
                        }
                    });
                }
            };

            var appends = (List<LiveChat> items) =>
            {
                var order = items
                    .OrderBy(it => it.TimestampUsec).ToList();
                if (order.Count > 0)
                {
                    var now = DateTime.UtcNow;
                    var delay = video._timeoutMs / order.Count;
                    var timeout = delay;
                    foreach (var item in order)
                    {
                        queue.Enqueue((item, now.AddMilliseconds(timeout)));
                        timeout += delay;
                    }
                    runTask();
                }
            };

            var getChatsAsync = () =>
            {
                if (isTop)
                    return video.GetTopChatsListAsync(urlOrVideoId, token);
                return video.GetLiveChatsListAsync(urlOrVideoId, token);
            };

            var getNextChatsAsync = () =>
            {
                if (isTop)
                    return video.GetNextTopChatsListAsync(token);
                return video.GetNextLiveChatsListAsync(token);
            };

            var isReplay = () =>
            {
                if (isTop)
                    return video._isTopReplay;
                return video._isReplay;
            };

            if (token.IsCancellationRequested)
                return;

            var liveChats = await getChatsAsync();
            if (isReplay())
                throw new Exception("Video is not live.");

            appends(liveChats);
            await delayWarp(video._timeoutMs, token);

            while (true)
            {
                if (token.IsCancellationRequested)
                    break;
                var nextLiveChats = await getNextChatsAsync();
                if (nextLiveChats == null)
                    break;
                appends(nextLiveChats);
                await delayWarp(video._timeoutMs, token);
            }

            while (isRun)
                await Task.Delay(1);
        }
    }
}
