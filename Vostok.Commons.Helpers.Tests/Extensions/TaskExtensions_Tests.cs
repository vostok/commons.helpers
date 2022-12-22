using System.Threading;
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
        public void TryWaitAsync_timeout_should_wait_for_task()
        {
            var task = Task.Delay(0.5.Seconds());
            task.TryWaitAsync(10.Seconds()).ShouldCompleteIn(1.Seconds()).Should().BeTrue();
        }

        [Test]
        public void TryWaitAsync_token_should_wait_for_task()
        {
            var task = Task.Delay(0.5.Seconds());
            using var cancel = new CancellationTokenSource();
            task.TryWaitAsync(cancel.Token).ShouldCompleteIn(1.Seconds()).Should().BeTrue();
        }
        
        [Test]
        public void TryWaitAsync_timeout_token_should_wait_for_task()
        {
            var task = Task.Delay(0.5.Seconds());
            using var cancel = new CancellationTokenSource();
            task.TryWaitAsync(10.Seconds(), cancel.Token).ShouldCompleteIn(1.Seconds()).Should().BeTrue();
        }

        [Test]
        public void TryWaitAsync_timeout_should_not_wait_for_completed_task()
        {
            var wait = Task.CompletedTask.TryWaitAsync(10.Seconds());
            wait.ShouldCompleteImmediately();
            wait.Result.Should().BeTrue();
        }
        
        [Test]
        public void TryWaitAsync_token_should_not_wait_for_completed_task()
        {
            using var cancel = new CancellationTokenSource();
            var wait = Task.CompletedTask.TryWaitAsync(cancel.Token);
            wait.ShouldCompleteImmediately();
            wait.Result.Should().BeTrue();
        }

        [Test]
        public void TryWaitAsync_timeout_token_should_not_wait_for_completed_task()
        {
            using var cancel = new CancellationTokenSource();
            var wait = Task.CompletedTask.TryWaitAsync(10.Seconds(), cancel.Token);
            wait.ShouldCompleteImmediately();
            wait.Result.Should().BeTrue();
        }

        [Test]
        public void TryWaitAsync_token_should_not_wait_for_canceled_token()
        {
            var task = Task.Delay(10.Seconds());

            var cancel = new CancellationTokenSource();
            cancel.Cancel();

            var wait = task.TryWaitAsync(cancel.Token);
            wait.ShouldCompleteImmediately();
            wait.Result.Should().BeFalse();
        }
        
        [Test]
        public void TryWaitAsync_timeout_token_should_not_wait_for_canceled_token()
        {
            var task = Task.Delay(10.Seconds());

            var cancel = new CancellationTokenSource();
            cancel.Cancel();

            var wait = task.TryWaitAsync(10.Seconds(), cancel.Token);
            wait.ShouldCompleteImmediately();
            wait.Result.Should().BeFalse();
        }

        [Test]
        public void TryWaitAsync_timeout_should_wait_with_timeout()
        {
            var task = Task.Delay(10.Seconds());
            task.TryWaitAsync(0.5.Seconds()).ShouldCompleteIn(1.Seconds()).Should().BeFalse();
            task.ShouldNotCompleteIn(0.5.Seconds());
        }

        [Test]
        public void TryWaitAsync_timeout_token_should_wait_with_timeout()
        {
            var task = Task.Delay(10.Seconds());
            using var cancel = new CancellationTokenSource();
            task.TryWaitAsync(0.5.Seconds(), cancel.Token).ShouldCompleteIn(1.Seconds()).Should().BeFalse();
            task.ShouldNotCompleteIn(0.5.Seconds());
        }

        [Test]
        public void TryWaitAsync_token_should_wait_with_token()
        {
            var task = Task.Delay(10.Seconds());
            var cancel = new CancellationTokenSource(0.5.Seconds());
            task.TryWaitAsync(cancel.Token).ShouldCompleteIn(1.Seconds()).Should().BeFalse();
            task.ShouldNotCompleteIn(0.5.Seconds());
        }
        
        [Test]
        public void TryWaitAsync_timeout_token_should_wait_with_token()
        {
            var task = Task.Delay(10.Seconds());
            var cancel = new CancellationTokenSource(0.5.Seconds());
            task.TryWaitAsync(5.Seconds(), cancel.Token).ShouldCompleteIn(1.Seconds()).Should().BeFalse();
            task.ShouldNotCompleteIn(0.5.Seconds());
        }
    }
}