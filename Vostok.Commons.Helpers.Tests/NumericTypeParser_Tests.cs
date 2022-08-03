using System;
using FluentAssertions;
using NUnit.Framework;

namespace Vostok.Commons.Helpers.Tests
{
    [TestFixture]
    public class NumericTypeParser_Tests
    {
        [TestCase("1.23", 1.23)]
        [TestCase("1,23", 1.23)]
        [TestCase(" -1 000,23 ", -1000.23)]
        [TestCase("5,12e2", 512)]
        [TestCase("512e-2", 5.12)]
        [TestCase("1", 1)]
        public void Should_parse_double_in_different_formats(string input, double expected)
        {
            NumericTypeParser<double>.TryParse(input, out var result).Should().BeTrue();

            result.Should().BeApproximately(expected, 0.0001);
        }

        [TestCase("1.23", 1.23)]
        [TestCase("1,23", 1.23)]
        [TestCase(" -1 000,23 ", -1000.23)]
        [TestCase("5,12e2", 512)]
        [TestCase("512e-2", 5.12)]
        public void Should_parse_decimal_in_different_formats(string input, double expected)
        {
            NumericTypeParser<decimal>.TryParse(input, out var result).Should().BeTrue();

            result.Should().BeApproximately((decimal)expected, 0.0001m);
        }

        [TestCase("1.23", 1.23f)]
        [TestCase("1,23", 1.23f)]
        [TestCase(" -1 000,23 ", -1000.23f)]
        [TestCase("5,12e2", 512f)]
        [TestCase("512e-2", 5.12f)]
        public void Should_parse_float_in_different_formats(string input, float expected)
        {
            NumericTypeParser<float>.TryParse(input, out var result).Should().BeTrue();

            result.Should().BeApproximately(expected, 0.0001f);
        }

        [Test]
        public void TryParse_should_throw_exception_for_invalid_number_type()
        {
            new Action(() => NumericTypeParser<object>.TryParse("123", out _)).Should().Throw<NotSupportedException>();
        }

        [TestCase("abc")]
        [TestCase("")]
        [TestCase(null)]
        public void TryParse_should_return_false_for_invalid_input(string input)
        {
            NumericTypeParser<double>.TryParse(input, out _).Should().BeFalse();
        }

        [Test]
        public void Parse_should_throw_exception_on_failure()
        {
            new Action(() => NumericTypeParser<double>.Parse("abc")).Should().Throw<FormatException>();
        }
    }
}