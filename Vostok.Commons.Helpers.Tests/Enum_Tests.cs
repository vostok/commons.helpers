using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Vostok.Commons.Helpers.Tests
{
    [TestFixture]
    public class Enum_Tests
    {
        [Test]
        public void Parse_should_correctly_parse_enum_values()
        {
            Enum<TestEnum>.Parse("One").Should().Be(TestEnum.One);
            Enum<TestEnum>.Parse("Two").Should().Be(TestEnum.Two);
            Enum<TestEnum>.Parse("Six").Should().Be(TestEnum.Six);
        }

        [Test]
        public void Parse_should_throw_FormatException_given_invalid_values()
        {
            new Action(() => Enum<TestEnum>.Parse("foo")).Should().Throw<FormatException>();
        }

        [Test]
        public void Parse_should_throw_InvalidOperationException_when_called_on_non_enum()
        {
            new Action(() => Enum<object>.Parse("foo")).Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Parse_should_not_ignore_case_by_default()
        {
            new Action(() => Enum<TestEnum>.Parse("one")).Should().Throw<FormatException>();
        }

        [Test]
        public void Parse_should_ignore_case_if_configured_so()
        {
            Enum<TestEnum>.Parse("one", true).Should().Be(TestEnum.One);
        }

        [Test]
        public void TryParse_should_correctly_parse_enum_values()
        {
            Enum<TestEnum>.TryParse("One", out var one);
            Enum<TestEnum>.TryParse("Two", out var two);
            Enum<TestEnum>.TryParse("Six", out var six);

            one.Should().Be(TestEnum.One);
            two.Should().Be(TestEnum.Two);
            six.Should().Be(TestEnum.Six);
        }

        [Test]
        public void TryParse_should_return_true_for_valid_values()
        {
            Enum<TestEnum>.TryParse("One", out _).Should().BeTrue();
            Enum<TestEnum>.TryParse("Two", out _).Should().BeTrue();
            Enum<TestEnum>.TryParse("Six", out _).Should().BeTrue();
        }

        [Test]
        public void TryParse_should_return_false_for_invalid_values()
        {
            Enum<TestEnum>.TryParse("foo", out _).Should().BeFalse();
        }

        [Test]
        public void TryParse_should_throw_InvalidOperationException_when_called_on_non_enum()
        {
            new Action(() => Enum<object>.TryParse("foo", out _)).Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void TryParse_should_not_ignore_case_by_default()
        {
            Enum<TestEnum>.TryParse("one", out _).Should().BeFalse();
        }

        [Test]
        public void TryParse_should_ignore_case_if_configured_so()
        {
            Enum<TestEnum>.TryParse("one", out var one, true).Should().BeTrue();
            one.Should().Be(TestEnum.One);
        }

        [Test]
        public void GetValues_should_return_typed_values()
        {
            var expectedValues = Enum.GetValues(typeof(TestEnum)).Cast<TestEnum>();

            var result = Enum<TestEnum>.GetValues();

            result.Should().BeEquivalentTo(expectedValues);
        }

        private enum TestEnum
        {
            One = 1,
            Two = 2,
            Six = 6
        }
    }
}