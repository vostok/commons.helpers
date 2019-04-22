using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Commons.Helpers.Url
{
    internal static class UrlParser
    {
        public static Uri[] Parse(IEnumerable<string> urls)
        {
            return urls?.Select(Parse).Where(u => u != null).ToArray();
        }

        public static Uri Parse(string url)
        {
            return !Uri.TryCreate(url, UriKind.Absolute, out var parsed)
                ? null
                : parsed;
        }
    }
}