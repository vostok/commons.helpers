#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Spans;

namespace Vostok.Commons.Helpers.Tests.Spans;

public class SpanSplitter_Test
{
    [Test]
    public void Test_MultipleSeparators([Values(true, false)] bool removeEmpty)
    {
        var o = removeEmpty;
        Check("aXb", "XYZ", o);
        Check("aXYZb", "XYZ", o);
        Check("a b", "XYZ", o);
    }

    [Test]
    public void Test_EmptySeparatorList([Values(true, false)] bool removeEmpty)
    {
        var o = removeEmpty;
        Check("", "", o);
        Check("a", "", o);
        Check("abcd", "", o);
        Check("ab", "", o);
    }

    [Test]
    public void Test_no_consecutive_separators([Values(true, false)] bool removeEmpty)
    {
        var o = removeEmpty;
        Check("a b", " ", o);
        Check("a b c", " ", o);

        Check("ab", " ", o);
        Check("a", " ", o);
        Check("", " ", o);

        Check("ab c", " ", o);
        Check("ab cd", " ", o);

        Check(" a", " ", o);
        Check(" ab", " ", o);

        Check("a ", " ", o);
        Check("ab ", " ", o);

        Check(" ", " ", o);
    }

    [Test]
    public void Test_Has_consecutive_separators([Values(true, false)] bool removeEmpty)
    {
        var o = removeEmpty;
        Check("a  b", " ", o);
        Check("a  b  c", " ", o);

        Check(" ", " ", o);
        Check("  ", " ", o);

        Check("ab   c", " ", o);
        Check("ab  cd", " ", o);

        Check("  a", " ", o);
        Check("  ab", " ", o);

        Check("a  ", " ", o);
        Check("ab  ", " ", o);
    }

    private void Check(string src, string delims, bool removeEmptyEntries)
    {
        var list = new List<string>();
        foreach (var span in src.AsSpan().Split(delims.AsSpan(), removeEmptyEntries)) list.Add(span.ToString());

        var splitOptions = removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;
        var expected = src.Split(delims.ToCharArray(), splitOptions);
        list.Should().BeEquivalentTo(expected, o => o.WithStrictOrdering());
    }
}
#endif