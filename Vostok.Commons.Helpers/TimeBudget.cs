using System;
using System.Diagnostics;
using Vostok.Commons.Helpers.Extensions;

namespace Vostok.Commons.Helpers
{
    internal class TimeBudget
    {
        public static readonly TimeBudget Infinite = new TimeBudget(TimeSpan.FromHours(1));
        public static readonly TimeBudget Expired = new TimeBudget(TimeSpan.Zero);

        public static TimeBudget StartNew(TimeSpan budget, TimeSpan precision) =>
            new TimeBudget(budget, precision).Start();

        public static TimeBudget StartNew(TimeSpan budget) =>
            new TimeBudget(budget).Start();

        public static TimeBudget StartNew(int budgetMs, int precisionMs) =>
            new TimeBudget(TimeSpan.FromMilliseconds(budgetMs), TimeSpan.FromMilliseconds(precisionMs)).Start();

        public static TimeBudget StartNew(int budgetMs) =>
            new TimeBudget(TimeSpan.FromMilliseconds(budgetMs)).Start();

        private readonly Stopwatch watch;

        public TimeBudget(TimeSpan budget, TimeSpan precision)
        {
            Budget = budget;
            Precision = precision;
            watch = new Stopwatch();
        }

        public TimeBudget(TimeSpan budget)
            : this(budget, TimeSpan.FromMilliseconds(5))
        {
        }

        public TimeSpan Budget { get; }

        public TimeSpan Precision { get; }

        public TimeBudget Start()
        {
            watch.Start();
            return this;
        }

        public TimeSpan Remaining()
        {
            var remaining = Budget - watch.Elapsed;
            return remaining < Precision
                ? TimeSpan.Zero
                : remaining;
        }

        public TimeSpan Elapsed() => watch.Elapsed;

        public TimeSpan TryAcquireTime(TimeSpan neededTime) =>
            TimeSpanExtensions.Min(neededTime, Remaining());

        public bool HasExpired() => Remaining() <= TimeSpan.Zero;
    }
}