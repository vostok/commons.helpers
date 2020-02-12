using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Disposable;

namespace Vostok.Commons.Helpers.Tests.Disposable
{
    [TestFixture]
    internal class ActionDisposable_Tests
    {
        [Test]
        public void Should_call_dispose_action_once()
        {
            var x = 0;
            var d = new ActionDisposable(() => x++);

            x.Should().Be(0);

            d.Dispose();

            x.Should().Be(1);

            d.Dispose();

            x.Should().Be(1);
        }
    }
}