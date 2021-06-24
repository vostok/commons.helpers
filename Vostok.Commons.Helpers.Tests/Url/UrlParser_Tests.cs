using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Url;

namespace Vostok.Commons.Helpers.Tests.Url
{
    [TestFixture]
    internal class UrlParser_Tests
    {
        [Test]
        public void Parse_should_parse_url()
        {
            UrlParser.Parse("http://vm-app1:4740/").Should().BeEquivalentTo(new Uri("http://vm-app1:4740/"));
        }

        [Test]
        public void Parse_should_not_parse_bad_url()
        {
            UrlParser.Parse("vm-name(16068)").Should().BeNull();
        }

        [Test]
        public void Parse_should_not_parse_null_url()
        {
            UrlParser.Parse((string)null).Should().BeNull();
        }

        [Test]
        public void Parse_should_parse_urls()
        {
            var urls = new List<string> {"http://vm-app1:4740/", "http://vm-app2:4740/", "http://vm-app3:4741/", "http://host.com/", "http://host"};
            UrlParser.Parse(urls).Should().BeEquivalentTo(urls.Select(u => new Uri(u)));
        }

        [Test]
        public void Parse_should_not_parse_bad_urls()
        {
            var urls = new List<string> {"http://vm-app1:4740/", "vm-name(16068)"};
            UrlParser.Parse(urls).Should().BeEquivalentTo(urls.Take(1).Select(u => new Uri(u)));
        }

        [Test]
        public void Parse_should_not_parse_null_urls()
        {
            UrlParser.Parse((IEnumerable<string>)null).Should().BeNull();
        }

        [Test]
        public void Parse_should_not_parse_urls_without_scheme()
        {
            var urls = new List<string> {"vm-app1:4740/path/", "vm-app1:4740", "host.com", "host.domain.com", "host.com/path/", "clusterconfig", "clusterconfig/path"};
            UrlParser.Parse(urls).Should().BeEmpty();
        }
    }
}