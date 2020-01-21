using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Helpers.Comparers;

namespace Vostok.Commons.Helpers.Tests.Comparers
{
    [TestFixture]
    internal class DictionaryComparer_Tests
    {
        [Test]
        public void Should_return_true_for_equal_dictionaries()
        {
            var left = new Dictionary<string, int>
            {
                ["a"] = 42,
                ["b"] = 24
            };

            var right = new Dictionary<string, int>
            {
                ["b"] = 24,
                ["a"] = 42
            };

            DictionaryComparer<string, int>.Instance.Equals(left, right)
                .Should()
                .BeTrue();

            DictionaryComparer<string, int>.Instance.GetHashCode(left)
                .Should()
                .Be(DictionaryComparer<string, int>.Instance.GetHashCode(right));
        }

        [Test]
        public void Should_return_false_for_non_equal_dictionaries_extra_key()
        {
            var left = new Dictionary<string, int>
            {
                ["a"] = 42,
                ["b"] = 24
            };

            var right = new Dictionary<string, int>
            {
                ["b"] = 24
            };

            DictionaryComparer<string, int>.Instance.Equals(left, right)
                .Should()
                .BeFalse();
        }

        [Test]
        public void Should_return_false_for_non_equal_dictionaries_missing_key()
        {
            var left = new Dictionary<string, int>
            {
                ["a"] = 42
            };

            var right = new Dictionary<string, int>
            {
                ["b"] = 24,
                ["a"] = 42
            };

            DictionaryComparer<string, int>.Instance.Equals(left, right)
                .Should()
                .BeFalse();
        }

        [Test]
        public void Should_return_false_for_non_equal_dictionaries_different_values()
        {
            var left = new Dictionary<string, int>
            {
                ["a"] = 24,
                ["b"] = 42
            };

            var right = new Dictionary<string, int>
            {
                ["b"] = 24,
                ["a"] = 42
            };

            DictionaryComparer<string, int>.Instance.Equals(left, right)
                .Should()
                .BeFalse();
        }

        [Test]
        public void Should_works_with_custom_key_type()
        {
            var left = new Dictionary<MyClass, int>
            {
                [new MyClass()] = 24,
                [new MyClass()] = 25
            };

            var right = new Dictionary<MyClass, int>
            {
                [new MyClass()] = 24,
                [new MyClass()] = 25
            };

            DictionaryComparer<MyClass, int>.Instance.GetHashCode(left)
                .Should()
                .Be(DictionaryComparer<MyClass, int>.Instance.GetHashCode(right));

            DictionaryComparer<MyClass, int>.Instance.Equals(left, right)
                .Should()
                .BeFalse();
        }

        internal class MyClass
        {
        }
    }
}