using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Testing;

namespace Vostok.Commons.Helpers.Tests.Extensions
{
    [TestFixture]
    internal class TaskExtensions_Tests
    {
        [Test]
        public void WaitAsync_should_wait_for_task()
        {
            var task = Task.Delay(0.5.Seconds());
            task.WaitAsync(10.Seconds()).ShouldCompleteIn(1.Seconds()).Should().BeTrue();
        }

        [Test]
        public void WaitAsync_should_wait_with_timeout()
        {
            var task = Task.Delay(10.Seconds());
            task.WaitAsync(0.5.Seconds()).ShouldCompleteIn(1.Seconds()).Should().BeFalse();
            task.ShouldNotCompleteIn(0.5.Seconds());
        }
    }
}