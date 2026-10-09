using System;
using System.Collections.Concurrent;
#if! NET6_0_OR_GREATER
using System.Diagnostics;
#endif
using System.Net;
using System.Runtime.CompilerServices;
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
        private readonly long cacheTtlTimestampTicks;
        private readonly TimeSpan resolveTimeout;

        private readonly ConcurrentDictionary<string, (IPAddress[] addresses, long validTo)> cache;
        private readonly ConcurrentDictionary<string, Lazy<Task<IPAddress[]>>> initialUpdateTasks;

        private int isUpdatingNow;

        public DnsResolver(TimeSpan cacheTtl, TimeSpan resolveTimeout)
        {
            this.cacheTtl = cacheTtl;
            cacheTtlTimestampTicks = ConvertToTimestampTicks(cacheTtl);
            this.resolveTimeout = resolveTimeout;

            cache = new ConcurrentDictionary<string, (IPAddress[] addresses, long validTo)>(StringComparer.Ordinal);
            initialUpdateTasks = new ConcurrentDictionary<string, Lazy<Task<IPAddress[]>>>(StringComparer.Ordinal);
        }

        public IPAddress[] Resolve(string hostname, bool canWait)
        {
            var currentTime = GetTimestamp();

            if (cache.TryGetValue(hostname, out var cacheEntry))
            {
                if (cacheEntry.validTo < currentTime &&
                    Interlocked.CompareExchange(ref isUpdatingNow, 1, 0) == 0)
                {
                    StartResolveAndUpdateTask(hostname, currentTime);
                }

                return cacheEntry.addresses;
            }

            return HandleEmptyCache(hostname, currentTime, canWait);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IPAddress[] HandleEmptyCache(string hostname, long currentTime, bool canWait)
        {
            //(deniaa): Do not inline this method because it prevents from creating unnecessary lambda closures
            // in case item exists in cache.
            var resolveTaskLazy = initialUpdateTasks.GetOrAdd(
                hostname,
                s => new Lazy<Task<IPAddress[]>>(
                    () => ResolveAndUpdateCacheAsync(s, currentTime),
                    LazyThreadSafetyMode.ExecutionAndPublication));

            var resolveTask = resolveTaskLazy.Value;

            if (!canWait)
                return resolveTask.IsCompleted ? resolveTask.GetAwaiter().GetResult() : EmptyAddresses;

            return resolveTask.Wait(resolveTimeout)
                ? resolveTask.GetAwaiter().GetResult()
                : EmptyAddresses;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void StartResolveAndUpdateTask(string hostname, long currentTime)
        {
            //(deniaa): Do not inline this method because it prevents from creating unnecessary lambda closures
            // in case item exists in cache.
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

        [ItemCanBeNull]
        private static async Task<IPAddress[]> TryResolveInternal(string hostname)
        {
            try
            {
                return await Dns.GetHostAddressesAsync(hostname).ConfigureAwait(false);
            }
            catch
            {
                return null;
            }
        }

        private async Task<IPAddress[]> ResolveAndUpdateCacheAsync(string hostname, long currentTime)
        {
            var addresses = await TryResolveInternal(hostname).ConfigureAwait(false);
            if (addresses != null)
            {
                cache[hostname] = (addresses, currentTime + cacheTtlTimestampTicks);
            }

            return addresses ?? EmptyAddresses;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetTimestamp()
        {
#if NET6_0_OR_GREATER
            // ReSharper disable once RedundantNameQualifier
            // (deniaa): Other modules reference this file as a source and this lead to ambiguous invocation between System.Environment and Vostok.Environment.
            return System.Environment.TickCount64;
#else
            return Stopwatch.GetTimestamp();
#endif
        }

        private static long ConvertToTimestampTicks(TimeSpan interval)
        {
#if NET6_0_OR_GREATER
            return (long)interval.TotalMilliseconds;
#else
            return (long)(interval.TotalSeconds * Stopwatch.Frequency);
#endif
        }
    }
}