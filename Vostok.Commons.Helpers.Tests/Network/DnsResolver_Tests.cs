using System;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Network;

namespace Vostok.Commons.Helpers.Tests.Network
{
    [TestFixture]
    internal class DnsResolver_Tests
    {
        [Test]
        public void Should_resolve_something()
        {
            var host = Dns.GetHostName();
            var resolver = new DnsResolver(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(1));
            resolver.Resolve(host, true).Should().NotBeEmpty();
            resolver.Resolve(host, true).Should().NotBeEmpty();
            resolver.Resolve(host, true).Should().NotBeEmpty();
            resolver.Resolve(host, true).Should().NotBeEmpty();
        }
    }
}