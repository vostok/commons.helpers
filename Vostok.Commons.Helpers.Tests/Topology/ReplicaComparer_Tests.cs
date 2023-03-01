using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Topology;

namespace Vostok.Commons.Helpers.Tests.Topology
{
    [TestFixture]
    internal class ReplicaComparer_Tests
    {
        [Test]
        public void Should_compare_paths()
            => ShouldBeDifferent("http://replica/foo", "http://replica/bar");

        [Test]
        public void Should_not_compare_query_parameters()
            => ShouldBeEqual("http://replica?a=b", "http://replica?c=d");

        [Test]
        public void Should_be_case_insensitive()
            => ShouldBeEqual("http://replica/", "http://REPLICA/");

        [Test]
        public void Should_ignore_domain_names()
            => ShouldBeEqual("http://replica.domain1/", "http://replica.domain2.com/");

        [Test]
        public void Should_ignore_domain_names_for_hostnames_starting_with_digit() 
            => ShouldBeEqual("http://1replica.domain1/", "http://1replica.domain2/");

        [Test]
        public void Should_differentiate_replicas_by_host_name()
        {
            ShouldBeDifferent("http://replica", "http://rep");
            ShouldBeDifferent("http://replica1", "http://replica2");
        }

        [Test]
        public void Should_differentiate_replicas_by_port()
            => ShouldBeDifferent("http://replica:81", "http://replica:82");

        [Test]
        public void Should_recognize_equal_ip_addresses()
            => ShouldBeEqual("http://1.2.3.4/", "http://1.2.3.4/");

        [TestCase("1234.0.0.1")]
        [TestCase("256.0.0.1")]
        [TestCase("1a.2b.3c.1")]
        public void Should_recognize_incorrect_ip_addresses(string address) 
            => ShouldBeEqual($"http://{address}/", $"http://{address + "0"}/");

        [TestCase("1")]
        [TestCase("1.2")]
        [TestCase("1.2.3")]
        [TestCase("127.0.255.25")]
        public void Should_recognize_correct_ip_addresses(string address) 
            => ShouldBeDifferent($"http://{address}/", $"http://{address + "0"}");

        private static void ShouldBeEqual(string replica1, string replica2)
        {
            Check(replica1, replica2, true);
            Check(replica2, replica1, true);
        }

        private static void ShouldBeDifferent(string replica1, string replica2)
        {
            Check(replica1, replica2, false);
            Check(replica2, replica1, false);
        }

        private static void Check(string replica1, string replica2, bool shouldBeEqual)
        {
            var url1 = new Uri(replica1);
            var url2 = new Uri(replica2);

            ReplicaComparer.Instance.Equals(url1, url2).Should().Be(shouldBeEqual);

            var hash1 = ReplicaComparer.Instance.GetHashCode(url1);
            var hash2 = ReplicaComparer.Instance.GetHashCode(url2);

            (hash1 == hash2).Should().Be(shouldBeEqual);
        }
    }
}