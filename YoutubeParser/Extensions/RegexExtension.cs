﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace YoutubeParser.Extensions
{
    internal static class RegexExtension
    {
        public static TResult Select<TResult>(this Match match, Func<Match, TResult> selector)
        {
            return selector(match);
        }

        public static IEnumerable<TResult> SelectMany<TResult>(this MatchCollection matches, Func<Match, TResult> selector)
        {
            foreach (Match match in matches)
            {
                yield return selector(match);
            }
        }

        public static IEnumerable<TResult> SelectMany<TResult>(this MatchCollection matches, Func<Match, int, TResult> selector)
        {
            var index = 0;
            foreach (Match match in matches)
            {
                yield return selector(match, index);
                index++;
            }
        }
    }
}
