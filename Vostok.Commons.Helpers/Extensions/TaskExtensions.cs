﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Commons.Helpers.Extensions
{
    internal static class TaskExtensions
    {
        public static Task SilentlyContinue(this Task source) => source.ContinueWith(_ => {});

        /// <summary>
        /// <para>Returns true if <paramref name="task"/> finished within given <paramref name="timeout"/>.</para>
        /// <para>Does not stop or cancel given <paramref name="task"/></para>
        /// </summary>
        public static async Task<bool> WaitAsync(this Task task, TimeSpan timeout)
        {
            using (var cts = new CancellationTokenSource())
            {
                var delay = Task.Delay(timeout, cts.Token);

                var result = await Task.WhenAny(task, delay).ConfigureAwait(false);
                if (result == delay)
                {
                    return false;
                }

                cts.Cancel();
            }

            return true;
        }
    }
}