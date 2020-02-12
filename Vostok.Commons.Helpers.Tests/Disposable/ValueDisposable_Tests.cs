using System;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Helpers.Disposable;

namespace Vostok.Commons.Helpers.Tests.Disposable
{
    [TestFixture]
    internal class ValueDisposable_Tests
    {
        [Test]
        public void Should_call_dispose_action_once()
        {
            var mock = Substitute.For<IDisposable>();
            var d = new ValueDisposable<int>(42, mock);

            mock.DidNotReceive().Dispose();

            d.Dispose();
            mock.Received().Dispose();
            mock.ClearReceivedCalls();

            d.Dispose();
            mock.DidNotReceive().Dispose();
        }
    }
}