using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeParser.Utils
{
    internal static class Memo
    {
        private static class For<T>
        {
            private static readonly ConditionalWeakTable<object, Dictionary<int, T>> CacheManifest = new();

            public static Dictionary<int, T> GetCache(object owner) =>
                CacheManifest.GetOrCreateValue(owner);
        }

        public static T Cache<T>(object owner, Func<T> getValue)
        {
            var cache = For<T>.GetCache(owner);
            var key = getValue.Method.GetHashCode();

            if (cache.TryGetValue(key, out var cachedValue))
                return cachedValue;

            var value = getValue();
            cache[key] = value;

            return value;
        }
    }
}
