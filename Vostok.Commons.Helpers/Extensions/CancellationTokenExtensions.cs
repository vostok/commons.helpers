using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Commons.Helpers.Extensions
{
    internal static class CancellationTokenExtensions
    {
        public static async Task WaitAsync(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (cancellationToken.Register(o => ((TaskCompletionSource<bool>)o).TrySetCanceled(), tcs))
            {
                await tcs.Task;
            }
        }
    }
}