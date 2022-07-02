using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YoutubeParser.Extensions
{
    internal static class YoutubeExtractorExtension
    {
        public static bool GetIsStream(this string publishedTime)
        {
            return publishedTime.Contains("Streamed");
        }

        public static long GetCountValue(this string viewCount)
        {
            var clearViewCount = viewCount
                .Pipe(it => Regex.Match(it, @"([\d\.,BKM]+)"))
                .Select(m => m.Groups[1].Value);
            var val = clearViewCount
                .Pipe(it => Regex.Match(it, @"([\d\.,]+)"))
                .Select(m => m.Groups[1].Value)
                .Pipe(it => it.Replace(",", ""))
                .Pipe(it => it == "" ? 0 : double.Parse(it));
            if (clearViewCount.Contains("B"))
                return (long)(val * 1000000000);
            if (clearViewCount.Contains("M"))
                return (long)(val * 1000000);
            if (clearViewCount.Contains("K"))
                return (long)(val * 1000);
            return (long)val;
        }

        public static TimeSpan? TryGetDuration(this string duration)
        {
            var formats = new string[]
                { @"m\:ss", @"mm\:ss", @"h\:mm\:ss", @"hh\:mm\:ss" };
            return TimeSpan.TryParseExact(
                duration, formats, DateTimeFormatInfo.InvariantInfo, out var result)
                    ? result : null;
        }

        public static DateTime? TryGetJoinedDate(this string joinedDate)
        {
            return DateTime.TryParse(joinedDate, DateTimeFormatInfo.InvariantInfo, 
                DateTimeStyles.None, out var result)
                    ? result : null;
        }

        public static DateTime? TryGetUploadDate(this string joinedDate)
        {
            return DateTime.TryParse(joinedDate, DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None, out var result)
                    ? result : null;
        }

        public static long GetPublishedTimeSeconds(this string publishedTime)
        {
            var val = publishedTime
                .Pipe(it => Regex.Match(it, @"(\d+)"))
                .Select(m => m.Groups[1].Value)
                .Pipe(it => it == "" ? 0 : int.Parse(it));

            var seconds = 0;
            if (publishedTime.Contains("second"))
                seconds = val * 1;
            if (publishedTime.Contains("minute"))
                seconds = val * 1 * 60;
            if (publishedTime.Contains("hour"))
                seconds = val * 1 * 60 * 60;
            if (publishedTime.Contains("day"))
                seconds = val * 1 * 60 * 60 * 24;
            if (publishedTime.Contains("week"))
                seconds = val * 1 * 60 * 60 * 24 * 7;
            if (publishedTime.Contains("month"))
                seconds = val * 1 * 60 * 60 * 24 * 30;
            if (publishedTime.Contains("year"))
                seconds = val * 1 * 60 * 60 * 24 * 365;

            return seconds;
        }
    }
}
