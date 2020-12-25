using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Vostok.Commons.Helpers.Network
{
    [PublicAPI]
    internal class DnsResolver
    {
        private static readonly IPAddress[] EmptyAddresses = {};

        private readonly TimeSpan cacheTtl;
        private readonly TimeSpan resolveTimeout;
        private readonly TimeSpan failsTtl;

        private readonly ConcurrentDictionary<string, (IPAddress[] addresses, DateTime validTo)> cache;
        private readonly ConcurrentDictionary<string, Lazy<Task<IPAddress[]>>> initialUpdateTasks;

        private int isUpdatingNow;

        public DnsResolver(TimeSpan cacheTtl, TimeSpan resolveTimeout, TimeSpan? failsTtl = null)
        {
            this.cacheTtl = cacheTtl;
            this.resolveTimeout = resolveTimeout;
            this.failsTtl = failsTtl ?? cacheTtl;

            cache = new ConcurrentDictionary<string, (IPAddress[] addresses, DateTime validTo)>(StringComparer.OrdinalIgnoreCase);
            initialUpdateTasks = new ConcurrentDictionary<string, Lazy<Task<IPAddress[]>>>(StringComparer.OrdinalIgnoreCase);
        }

        public IPAddress[] Resolve(string hostname, bool canWait)
        {
            var currentTime = DateTime.UtcNow;

            if (cache.TryGetValue(hostname, out var cacheEntry))
            {
                if (cacheEntry.validTo < currentTime &&
                    Interlocked.CompareExchange(ref isUpdatingNow, 1, 0) == 0)
                {
                    Task.Run(
                        async () =>
                        {
                            try
                            {
                                await ResolveAndUpdateCacheAsync(hostname, currentTime).ConfigureAwait(false);
                            }
                            finally
                            {
                                Interlocked.Exchange(ref isUpdatingNow, 0);
                            }
                        });
                }

                return cacheEntry.addresses;
            }

            var resolveTaskLazy = initialUpdateTasks.GetOrAdd(
                hostname,
                _ => new Lazy<Task<IPAddress[]>>(
                    () => ResolveAndUpdateCacheAsync(hostname, currentTime),
                    LazyThreadSafetyMode.ExecutionAndPublication));

            var resolveTask = resolveTaskLazy.Value;

            if (!canWait)
                return resolveTask.IsCompleted ? resolveTask.GetAwaiter().GetResult() : EmptyAddresses;

            return resolveTask.Wait(resolveTimeout)
                ? resolveTask.GetAwaiter().GetResult()
                : EmptyAddresses;
        }

        private static async Task<(bool isSuccessful, IPAddress[] addresses)> ResolveInternal(string hostname)
        {
            try
            {
                var addresses = await Dns.GetHostAddressesAsync(hostname).ConfigureAwait(false);
                return (true, addresses);
            }
            catch
            {
                return (false, EmptyAddresses);
            }
        }

        private async Task<IPAddress[]> ResolveAndUpdateCacheAsync(string hostname, DateTime currentTime)
        {
            var (isSuccessful, addresses) = await ResolveInternal(hostname).ConfigureAwait(false);
            var ttl = isSuccessful ? cacheTtl : failsTtl;
            cache[hostname] = (addresses, currentTime + ttl);
            return addresses;
        }
    }
}