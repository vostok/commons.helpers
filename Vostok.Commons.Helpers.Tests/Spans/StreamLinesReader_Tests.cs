using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
#if NETCOREAPP3_1_OR_GREATER
using Vostok.Commons.Helpers.Spans;

namespace Vostok.Commons.Helpers.Tests.Spans;

public class StreamLinesReader_Tests
{
    private const string R = "\r";
    private const string N = "\n";
    private const string B = "\r\n";

    private const string NonLineBreak = "\n\r"; //produce empty string!

    [Test]
    public void Test_ResetPosition()
    {
        var src1 = CreateContent("abc" + B + "z");

        var linesReader = new StreamLinesReader(src1, 4, 4);
        linesReader.TryReadLine(out _).Should().Be(true);

        src1.Position.Should().Be(4);

        linesReader.ResetToZero();

        linesReader.TryReadLine(out var span).Should().Be(true);
        span.ToString().Should().Be("abc");
        src1.Position.Should().Be(4);

        linesReader.TryReadLine(out span).Should().Be(true);
        span.ToString().Should().Be("z");
        src1.Position.Should().Be(6);
    }

    [Test]
    public void Test_CharBytesBreakedByBufferBorder_3([Values(1, 2, 4, 5)] int buf)
    {
        //https://en.wikipedia.org/wiki/UTF-8
        Check("abc\u20AC", buf); //u20AC is 3-byte in utf8 {E2 82 AC}
    }

    [Test]
    public void Test_CharBytesBreakedByBufferBorder_4([Values(4, 5, 6)] int buf)
    {
        Check("abc\u10348", buf); //u10348 is 4-byte in utf8 {F0 90 8D 88}
    }

    [Test]
    public void Test_CharBufferMaxSize()
    {
        Check("abc" + R + "abcd" + R + "1234567890" + R + "333", 10, 2, new[]
        {
            4,
            6,
            12,
            12
        });

        Check("abc" + R + "abcd" + R + "1234567890" + R + "333", 4, 2, new[]
        {
            4,
            6,
            12,
            12
        });

        Check("a" + R + "a" + R + "a" + R + "a", 11, 2, new[] //no resizes
        {
            2,
            2,
            2,
            2
        });
    }


    [Test]
    public void Test_SingleLine()
    {
        Check("abc");
        Check("a");
        Check("");
    }

    [Test]
    public void Test_DifferentLineBreaks()
    {
        Check("abc" + R + "d");
        Check("abc" + N + "d");
        Check("abc" + B + "d");
    }

    [Test]
    public void Test_SequentialRNs()
    {
        Check(B);
        Check(B + B);
        Check(B + B + B);
        Check("abc" + B);
        Check(B + "abc");
        Check(B + B + "abc");
        Check("abc" + B + B);
        Check("a" + B + B);
        Check(B + B + "a");
    }

    [Test]
    public void Test_NonLB()
    {
        Check("abc" + NonLineBreak + "d");
        Check("a" + NonLineBreak + "d");

        Check("abc" + NonLineBreak);
        Check("a" + NonLineBreak);

        Check(NonLineBreak + "abc");
        Check(NonLineBreak + "a");
    }

    [Test]
    public void Test_EmptyLines()
    {
        Check("abc" + R + R + "d");
        Check("a" + R + R + "de");
        Check("a" + R + R + "d");

        Check("abc" + N + N + "d");
        Check("a" + N + N + "de");
        Check("a" + N + N + "d");


        Check("abc" + R + R);
        Check("a" + R + R);
        Check(R + R + "a");
        Check(R);
        Check(R + R);
        Check(R + R + R);

        Check("a" + N + N);
        Check("abc" + N + N);
        Check(N + N + "a");
        Check(N);
        Check(N + N);
        Check(N + N + N);
    }

    [Test]
    public void Test_SmallByteBuffer()
    {
        var cb = 10;
        Check("abc" + R + "def", 1, cb);
        Check("a", 1, cb);
        Check("", 1, cb);
        Check(R, 1, cb);
        Check(B, 1, cb);
    }

    [Test]
    public void Test_SmallByteBufferAndCharBuffer()
    {
        var cb = 2;
        Check("abc" + R + "def", 1, cb);
        Check("a", 1, cb);
        Check("", 1, cb);
        Check("abcdefghik" + B, 1, cb);
    }

    [Test]
    public void Test_SmallCharBuffer()
    {
        var cb = 2;
        var bb = 100;
        Check("abc" + R + "def", bb, cb);
        Check("a", bb, cb);
        Check("", bb, cb);
        Check("abcdefghik" + B, bb, cb);
    }

    private void Check(string s, int byteBufferSize = 10, int charsBufferSize = 10, int[] charBufSizesEachStep = null)
    {
        var src1 = CreateContent(s);
        var linesReader = new StreamLinesReader(src1, byteBufferSize, charsBufferSize);
        var referenceReader = new StringReader(s);

        var expected = new List<string>();

        var line = "";
        while ((line = referenceReader.ReadLine()) != null)
        {
            expected.Add(line);
        }

        if (charBufSizesEachStep != null)
            charBufSizesEachStep.Length.Should().Be(expected.Count, "Incorrect charBufSizesEachStep count");

        var actual = new List<string>();

        var i = 0;
        while (linesReader.TryReadLine(out var lineSpan))
        {
            actual.Add(lineSpan.ToString());
            if (charBufSizesEachStep != null)
                linesReader.CharBufferSize.Should().Be(charBufSizesEachStep[i], $"string index={i}, s={actual.Last()}");
            i++;
        }


        actual.Should().BeEquivalentTo(expected, o => o.WithStrictOrdering());
    }


    private Stream CreateContent(string s)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(s));
    }
}

#endif