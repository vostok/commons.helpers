using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vostok.Commons.Helpers.Extensions
{
    internal static class IEnumerableExtensions
    {
        public static async Task<IEnumerable<TOut>> LoopAsync<TIn, TOut>(this IEnumerable<TIn> enumerable, Func<TIn, Task<TOut>> selector) =>
            await Task.WhenAll(enumerable.Select(selector)).ConfigureAwait(false);
    }
}