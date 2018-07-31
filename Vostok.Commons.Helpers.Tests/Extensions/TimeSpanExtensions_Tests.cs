using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Extensions;

namespace Vostok.Commons.Helpers.Tests.Extensions
{
    [TestFixture]
    public class TimeSpanExtensions_Tests
    {
        [Test]
        public void Should_Multiply()
        {
            new TimeSpan(10).Multiply(10).Should().Be(new TimeSpan(100));
        }

        [Test]
        public void Should_Divide()
        {
            new TimeSpan(10).Divide(2).Should().Be(new TimeSpan(5));
        }

        [TestCase(10, 20, 10)]
        [TestCase(20, 10, 10)]
        [TestCase(10, 10, 10)]
        public void Should_choose_Min(int val1, int val2, int res)
        {
            TimeSpanExtensions.Min(new TimeSpan(val1), new TimeSpan(val2)).Should().Be(new TimeSpan(res));
        }

        [TestCase(10, 20, 20)]
        [TestCase(20, 10, 20)]
        [TestCase(10, 10, 10)]
        public void Should_choose_Max(int val1, int val2, int res)
        {
            TimeSpanExtensions.Max(new TimeSpan(val1), new TimeSpan(val2)).Should().Be(new TimeSpan(res));
        }

        [TestCase(10, 8, 0, 0, 0, "10.333 days")]
        [TestCase(0, 10, 30, 0, 0, "10.5 hours")]
        [TestCase(0, 0, 10, 30, 0, "10.5 minutes")]
        [TestCase(0, 0, 0, 10, 123, "10.123 seconds")]
        [TestCase(0, 0, 0, 0, 10, "10 milliseconds")]
        [TestCase(0, 0, 0, 0, 0, "00:00:00")]
        public void Should_turn_ToPrettyString(int d, int h, int m, int s, int ms, string res)
        {
            new TimeSpan(d, h, m, s, ms).ToPrettyString().Should().Be(res);
        }
    }
}