using System.Collections.Generic;

namespace Vostok.Commons.Helpers.Extensions
{
    internal static class DictionaryExtensions
    {
        public static TValue GetValueOrNull<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class =>
            dictionary != null && dictionary.TryGetValue(key, out var value) ? value : null;
    }
}