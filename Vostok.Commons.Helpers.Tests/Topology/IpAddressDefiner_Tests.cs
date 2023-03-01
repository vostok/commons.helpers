using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Topology;

namespace Vostok.Commons.Helpers.Tests.Topology
{
    public class IpAddressDefiner_Tests
    {
        [TestCase("1")]
        [TestCase("1.2")]
        [TestCase("1.2.3")]
        [TestCase("127.0.0.0")]
        [TestCase("127.0.0.255")]
        [TestCase("123:123:123:123:123:123")]
        [TestCase(":")]
        public void Test(string address)
        {
            IpAddressDefiner.IsIpAddress(address).Should().BeTrue();
        }

        [TestCase("1234.0.0.1")]
        [TestCase("256.0.0.1")]
        [TestCase("not-ip")]
        [TestCase("1a.2b.3c.4d")]
        [TestCase("0xF.0xF.0xF.0xF")]
        [TestCase("...")]
        [TestCase("123..32.")]
        [TestCase(".32.32.")]
        public void Test2(string address)
        {
            IpAddressDefiner.IsIpAddress(address).Should().BeFalse();
        }
    }
}