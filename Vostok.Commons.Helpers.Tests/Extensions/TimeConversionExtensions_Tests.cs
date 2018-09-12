using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Extensions;

namespace Vostok.Commons.Helpers.Tests.Extensions
{
    [TestFixture]
    public class TimeConversionExtensions_Tests
    {
        [Test]
        public void Should_turn_int_into_Ticks()
        {
            const int val = 123;
            val.Ticks().Should().Be(TimeSpan.FromTicks(val));
        }

        [Test]
        public void Should_turn_long_into_Ticks()
        {
            const long val = 123;
            val.Ticks().Should().Be(TimeSpan.FromTicks(val));
        }

        [Test]
        public void Should_turn_ushort_into_Milliseconds()
        {
            const ushort val = 123;
            val.Milliseconds().Should().Be(TimeSpan.FromMilliseconds(val));
        }

        [Test]
        public void Should_turn_int_into_Milliseconds()
        {
            const int val = 123;
            val.Milliseconds().Should().Be(TimeSpan.FromMilliseconds(val));
        }

        [Test]
        public void Should_turn_long_into_Milliseconds()
        {
            const long val = 123;
            val.Milliseconds().Should().Be(TimeSpan.FromMilliseconds(val));
        }

        [Test]
        public void Should_turn_double_into_Milliseconds()
        {
            const double val = 1.23;
            val.Milliseconds().Should().Be(TimeSpan.FromMilliseconds(val));
        }

        [Test]
        public void Should_turn_ushort_into_Seconds()
        {
            const ushort val = 123;
            val.Seconds().Should().Be(TimeSpan.FromSeconds(val));
        }

        [Test]
        public void Should_turn_int_into_Seconds()
        {
            const int val = 123;
            val.Seconds().Should().Be(TimeSpan.FromSeconds(val));
        }

        [Test]
        public void Should_turn_long_into_Seconds()
        {
            const long val = 123;
            val.Seconds().Should().Be(TimeSpan.FromSeconds(val));
        }

        [Test]
        public void Should_turn_double_into_Seconds()
        {
            const double val = 1.23;
            val.Seconds().Should().Be(TimeSpan.FromSeconds(val));
        }

        [Test]
        public void Should_turn_ushort_into_Minutes()
        {
            const ushort val = 123;
            val.Minutes().Should().Be(TimeSpan.FromMinutes(val));
        }

        [Test]
        public void Should_turn_int_into_Minutes()
        {
            const int val = 123;
            val.Minutes().Should().Be(TimeSpan.FromMinutes(val));
        }

        [Test]
        public void Should_turn_long_into_Minutes()
        {
            const long val = 123;
            val.Minutes().Should().Be(TimeSpan.FromMinutes(val));
        }

        [Test]
        public void Should_turn_double_into_Minutes()
        {
            const double val = 1.23;
            val.Minutes().Should().Be(TimeSpan.FromMinutes(val));
        }

        [Test]
        public void Should_turn_ushort_into_Hours()
        {
            const ushort val = 123;
            val.Hours().Should().Be(TimeSpan.FromHours(val));
        }

        [Test]
        public void Should_turn_int_into_Hours()
        {
            const int val = 123;
            val.Hours().Should().Be(TimeSpan.FromHours(val));
        }

        [Test]
        public void Should_turn_long_into_Hours()
        {
            const long val = 123;
            val.Hours().Should().Be(TimeSpan.FromHours(val));
        }

        [Test]
        public void Should_turn_double_into_Hours()
        {
            const double val = 1.23;
            val.Hours().Should().Be(TimeSpan.FromHours(val));
        }

        [Test]
        public void Should_turn_ushort_into_Days()
        {
            const ushort val = 123;
            val.Days().Should().Be(TimeSpan.FromDays(val));
        }

        [Test]
        public void Should_turn_int_into_Days()
        {
            const int val = 123;
            val.Days().Should().Be(TimeSpan.FromDays(val));
        }

        [Test]
        public void Should_turn_long_into_Days()
        {
            const long val = 123;
            val.Days().Should().Be(TimeSpan.FromDays(val));
        }

        [Test]
        public void Should_turn_double_into_Days()
        {
            const double val = 1.23;
            val.Days().Should().Be(TimeSpan.FromDays(val));
        }
    }
}