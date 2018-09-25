using System;

namespace Vostok.Commons.Helpers.Url
{
    internal static class UrlExtensions
    {
        public static string ToStringWithoutQuery(this Uri url)
        {
            var urlString = url.ToString();

            var queryBeginning = urlString.IndexOf("?", StringComparison.Ordinal);
            if (queryBeginning >= 0)
                urlString = urlString.Substring(0, queryBeginning);

            return urlString;
        }
    }
}