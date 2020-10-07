using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Time;

namespace Vostok.Commons.Helpers.Extensions
{
    internal static class TaskExtensions
    {
        public static Task<T> WithTimeout<T>(this Task<T> task, TimeBudget budget, string timeoutMessage) => task.WithTimeout(budget.Remaining, timeoutMessage);
        
        public static async Task<T> WithTimeout<T>(this Task<T> task, TimeSpan timeout, string timeoutMessage)
        {
            await ((Task) task).WithTimeout(timeout, timeoutMessage);

            return await task;
        }
        
        public static Task WithTimeout(this Task task, TimeBudget budget, string timeoutMessage) => task.WithTimeout(budget.Remaining, timeoutMessage);
        
        public static async Task WithTimeout(this Task task, TimeSpan timeout, string timeoutMessage)
        {
            if (!await task.WaitAsync(timeout))
                throw new TimeoutException(timeoutMessage);

            await task;
        }
        
        public static Task SilentlyContinue(this Task source) => source.ContinueWith(_ => {});

        /// <summary>
        /// <para>Returns true if <paramref name="task"/> finished within given <paramref name="timeout"/>.</para>
        /// <para>Does not stop or cancel given <paramref name="task"/></para>
        /// </summary>
        public static async Task<bool> WaitAsync(this Task task, TimeSpan timeout)
        {
            if (task.IsCompleted)
                return true;

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
