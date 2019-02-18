using System;
using System.Reactive.Linq;
using RxObservable = System.Reactive.Linq.Observable;

namespace Vostok.Commons.Helpers.Observable
{
    internal static class HealingObservable
    {
        public static IObservable<T> CatchErrors<T>(Func<IObservable<T>> observe, TimeSpan cooldown)
        {
            return observe().Catch<T, Exception>(_ => RxObservable.Defer(() => CatchErrors(observe, cooldown)).Delay(cooldown));
        }

        public static IObservable<(T, Exception)> PushErrors<T>(Func<IObservable<(T, Exception)>> observe, TimeSpan cooldown)
        {
            return observe()
                .Catch<(T, Exception), Exception>(e => RxObservable.Defer(() => PushErrors(observe, cooldown)).Delay(cooldown).StartWith((default, e)));
        }
    }
}