using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Network;

namespace Vostok.Commons.Helpers.Tests.Network
{
    internal class IPv4Network_Tests
    {
        [TestCaseSource(nameof(TestCasesForContains))]
        public bool Contains_should_be_correct(string networkAddress, int networkCidr, string ip)
            => new IPv4Network(IPAddress.Parse(networkAddress), (byte)networkCidr).Contains(IPAddress.Parse(ip));

        [TestCase("0.0.0.0", 0)]
        [TestCase("1.2.3.4", 31)]
        public void TryParse_should_parse_ipv4_network(string ip, int cidr)
        {
            IPv4Network.TryParse($"{ip}/{cidr}", out var network).Should().BeTrue();
            network.NetworkAddress.Should().BeEquivalentTo(IPAddress.Parse(ip));
            network.NetworkCidr.Should().Be((byte)cidr);
        }

        private static IEnumerable<TestCaseData> TestCasesForContains()
        {
            yield return new TestCaseData("0.0.0.0", 0, "8.8.8.8").Returns(true);
            yield return new TestCaseData("0.0.0.0", 0, "0.0.0.0").Returns(true);
            yield return new TestCaseData("0.0.0.0", 0, "255.255.255.255").Returns(true);

            yield return new TestCaseData("8.8.0.0", 16, "8.8.0.0").Returns(true);
            yield return new TestCaseData("8.8.0.0", 16, "8.8.255.255").Returns(true);
            yield return new TestCaseData("8.8.0.0", 16, "8.8.8.8").Returns(true);
            yield return new TestCaseData("8.8.0.0", 16, "8.7.255.255").Returns(false);
            yield return new TestCaseData("8.8.0.0", 16, "8.9.0.0").Returns(false);

            yield return new TestCaseData("8.8.15.0", 24, "8.8.15.0").Returns(true);
            yield return new TestCaseData("8.8.15.0", 24, "8.8.15.255").Returns(true);
            yield return new TestCaseData("8.8.15.0", 24, "8.8.15.15").Returns(true);
            yield return new TestCaseData("8.8.15.0", 24, "8.8.14.255").Returns(false);
            yield return new TestCaseData("8.8.15.0", 24, "8.8.16.0").Returns(false);

            yield return new TestCaseData("8.8.8.8", 31, "8.8.8.8").Returns(true);
            yield return new TestCaseData("8.8.8.8", 31, "8.8.8.9").Returns(true);
            yield return new TestCaseData("8.8.8.8", 31, "8.8.8.7").Returns(false);
            yield return new TestCaseData("8.8.8.8", 31, "8.8.8.10").Returns(false);

            yield return new TestCaseData("8.8.8.8", 32, "8.8.8.8").Returns(true);
            yield return new TestCaseData("8.8.8.8", 32, "8.8.8.7").Returns(false);
            yield return new TestCaseData("8.8.8.8", 32, "8.8.8.9").Returns(false);
        }
    }
}