using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Url;

namespace Vostok.Commons.Helpers.Tests.Url
{
    [TestFixture]
    internal class UrlNormalizer_Tests
    {
        [TestCase("", "/")]
        [TestCase("/", "/")]
        [TestCase("foo/bar/baz", "foo/bar/baz")]
        [TestCase("foo/bar/baz/", "foo/bar/baz")]
        [TestCase("/foo/bar/baz", "foo/bar/baz")]
        [TestCase("/foo/bar/baz/", "foo/bar/baz")]
        [TestCase("foo/1/bar", "foo/~/bar")]
        [TestCase("foo/5435453/bar", "foo/~/bar")]
        [TestCase("foo/-5435453/bar/", "foo/~/bar")]
        [TestCase("foo/123-456-789/bar", "foo/~/bar")]
        [TestCase("123/foo/bar", "~/foo/bar")]
        [TestCase("foo/bar/123", "foo/bar/~")]
        [TestCase("/foo/bar/%d0%b1%d1%83%d0%bb%d0%bb%d1%89%d0%b8%d1%82", "foo/bar/~")]
        [TestCase("/foo/left+right/bar/", "foo/~/bar")]
        [TestCase("/foo/0fab74ac/bar", "foo/~/bar")]
        [TestCase("/foo/0fab74/bar", "foo/0fab74/bar")]
        [TestCase("/foo/0fab74ac2/bar", "foo/~/bar")]
        [TestCase("/foo/bar/67CE9912-72E6-4B7F-AD6B-7EB7D20CEDEC", "foo/bar/~")]
        [TestCase("/foo/bar/67ce9912-72e6-4b7f-ad6b-7eb7d20cedec", "foo/bar/~")]
        [TestCase("authorizator/v1.1/1$d46563a5-cf90-4995-8f9e-388659477642/permissions", "authorizator/v1.1/~/permissions")]
        [TestCase("/v1/contents/srv/dd-tmp/nsid/public/pf8/4636/pv_e6eb854f-59d1-48f1-918e-2ba09e424761.ab4208a9-6237-47e9-a807-258f469ab33a.636558468599688976_.content", "v1/contents/srv/dd-tmp/nsid/public/pf8/~/~")]
        [TestCase("/regusers-sql-sm/reg_regusers/d86f85cc-e281-464c-874b-b95865dbb189;2bb7f717-21c3-4433-8411-e299df188d47;382e4ccf-f553-453e-84c7-55d1b4b04107", "regusers-sql-sm/reg_regusers/~")]
        [TestCase("//foo//bar//baz//", "foo/bar/baz")]
        [TestCase("FOO/BAR/BaZ", "foo/bar/baz")]
        [TestCase("foo/abcdefghikabcdefghikabcdefghikabcdefghikabcdefghikabcdefghikz/baz", "foo/~/baz")]
        [TestCase("foo/hex_0xa0b4f85d_hex/baz", "foo/~/baz")]
        [TestCase("foo/guid_EF27C618-1B7F-4496-90A9-4E80259F3E67_guid/baz", "foo/~/baz")]
        [TestCase("foo/~/~/bar", "foo/~/~/bar")]
        public void Should_successfully_normalize_url_with_given_path(string path, string expected)
        {
            var result1 = UrlNormalizer.NormalizePath(path);
            var result2 = UrlNormalizer.NormalizePath(new Uri(path.TrimStart('/'), UriKind.Relative));
            var result3 = UrlNormalizer.NormalizePath(new Uri(path.TrimStart('/') + "?a=b", UriKind.Relative));
            var result4 = UrlNormalizer.NormalizePath(new Uri($"http://localhost:322/{path.TrimStart('/')}", UriKind.Absolute));
            var result5 = UrlNormalizer.NormalizePath(new Uri($"http://localhost:322/{path.TrimStart('/')}?a=b", UriKind.Absolute));
            var result6 = UrlNormalizer.NormalizePath(result1);

            result1.Should().Be(expected);
            result2.Should().Be(expected);
            result3.Should().Be(expected);
            result4.Should().Be(expected);
            result5.Should().Be(expected);
            result6.Should().Be(expected);
        }

        [Test]
        public void Should_correctly_truncate_if_exceeding_max_length()
        {
            UrlNormalizer.NormalizePath("/foo/bar/baz/________________________", 30).Should().Be("foo/bar/baz/____...(truncated)");
        }

        [Test]
        public void Should_correctly_truncate_if_max_length_is_less_than_truncation_text()
        {
            UrlNormalizer.NormalizePath("foo/bar/baz", 3).Should().Be("...(truncated)");
        }
    }
}