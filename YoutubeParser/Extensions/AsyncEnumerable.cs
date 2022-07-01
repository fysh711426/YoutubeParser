using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeParser.Extensions
{
    public static class AsyncEnumerable
    {
#if (!NET45 && !NET46)
        public static async IAsyncEnumerable<TSource> Break<TSource>(this IAsyncEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            await foreach(var item in source)
            {
                if (predicate(item))
                    break;
                yield return item;
            }
        }
#endif
    }
}
