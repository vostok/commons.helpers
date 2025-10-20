using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace Vostok.Commons.Helpers.Url
{
    [PublicAPI]
    internal static class UrlNormalizer
    {
        private const char Slash = '/';
        private const char Tilde = '~';
        private const char Dash = '-';

        private const int DefaultLengthLimit = 100;

        private const string TruncatedText = "...(truncated)";

        private static readonly bool[] HexCharactersMap;
        private static readonly bool[] AllowedCharactersMap;
        

        [ThreadStatic]
        private static StringBuilder resultBuilder;

        static UrlNormalizer()
        {
            PrepareCharacterMaps(out HexCharactersMap, out AllowedCharactersMap);
        }

        public static string NormalizePath(Uri url, int maxLength = DefaultLengthLimit)
        {
            var str = url.IsAbsoluteUri ? url.AbsolutePath : url.ToStringWithoutQuery();
            return NormalizePath(str, 0, str.Length,  maxLength);
        }

        public static string NormalizePath(string path, int maxLength = DefaultLengthLimit)
        {
            return NormalizePath(path, 0, path.Length, maxLength);
        }

        public static string NormalizePath(string path, int offset, int stringLength, int maxLength = DefaultLengthLimit)
        {
            var builder = ObtainBuilder();

            var enumerator = new SegmentEnumerator(path, offset, stringLength);
            while (enumerator.MoveNext())
            {
                var segment = enumerator.Current;
                if (RequiresSubstitution(segment))
                {
                    builder.Append(Tilde);
                }
                else
                {
                    for (var i = 0; i < segment.Length; i++)
                    {
                        builder.Append(ToLowerFast(segment[i]));
                    }
                }

                if (builder.Length > maxLength)
                    break;

                builder.Append(Slash);
            }

            HandleTrailingSlash(builder);

            TruncateIfNeeded(builder, maxLength);

            return builder.ToString();
        }

        private static StringBuilder ObtainBuilder()
        {
            var builder = resultBuilder ?? (resultBuilder = new StringBuilder(64));

            builder.Clear();

            return builder;
        }

        private static bool RequiresSubstitution(Segment segment)
        {
            if (segment.IsEmpty)
                return false;

            if (segment.Length >= 40)
                return true;

            var hexRun = 0;
            var sawDigit = false;
            var dashOrDigitOnly = true;

            for (var i = 0; i < segment.Length; i++)
            {
                var c = segment[i];

                if (!IsAllowedCharacter(c))
                    return true;

                if (IsHexCharacter(c))
                {
                    if (++hexRun >= 8)
                        return true;
                }
                else
                {
                    hexRun = 0;
                }

                var isDigit = (c >= '0' && c <= '9');
                var isDash  = (c == Dash);
                if (!isDigit && !isDash)
                    dashOrDigitOnly = false;
                sawDigit |= isDigit;
            }

            if (dashOrDigitOnly && sawDigit)
                return true;

            return false;
        }

        private static void HandleTrailingSlash(StringBuilder builder)
        {
            if (builder.Length == 0)
            {
                builder.Append(Slash);
                return;
            }

            if (builder[builder.Length - 1] == Slash && builder.Length > 1)
                builder.Length--;
        }

        private static void TruncateIfNeeded(StringBuilder builder, int maxLength)
        {
            if (builder.Length <= maxLength)
                return;

            maxLength = Math.Max(maxLength, TruncatedText.Length);

            builder.Length = maxLength - TruncatedText.Length;

            builder.Append(TruncatedText);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char ToLowerFast(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                return (char)(c + 32);
            }

            return c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsHexCharacter(char c)
        {
            return c < 128 && HexCharactersMap[c];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAllowedCharacter(char c)
        {
            return c < 128 && AllowedCharactersMap[c];
        }
        
        private ref struct SegmentEnumerator
        {
            private readonly string source;
            private int pos;
            private int segmentStart;
            private readonly int maxOffset;

            public SegmentEnumerator(string source, int offset, int segmentLength)
            {
                this.source = source;
                pos = offset;
                segmentStart = offset;
                maxOffset = offset + segmentLength;
            }

            public bool MoveNext()
            {
                while (pos < maxOffset && source[pos] == Slash)
                    pos++;

                segmentStart = pos;

                while (pos < maxOffset && source[pos] != Slash)
                    pos++;

                return segmentStart < maxOffset;
            }

            public Segment Current => new Segment(source, segmentStart, pos - segmentStart);
        }

        #region Segment

        private struct Segment
        {
            public readonly int Length;

            private readonly string Path;
            private readonly int Offset;

            public Segment(string path, int offset, int length)
            {
                Path = path;
                Offset = offset;
                Length = length;
            }

            public bool IsEmpty => Length == 0;

            public char this[int index] => Path[Offset + index];
        }

        #endregion

        #region Characters

        private static void PrepareCharacterMaps(out bool[] hexMap, out bool[] allowedMap)
        {
            hexMap = new bool[128];

            foreach (var c in EnumerateHexCharacters())
            {
                hexMap[c] = true;
            }

            allowedMap = new bool[128];

            foreach (var c in EnumerateAllowedCharacters())
            {
                allowedMap[c] = true;
            }
        }

        private static IEnumerable<char> EnumerateHexCharacters()
        {
            for (var c = 'a'; c <= 'f'; c++)
                yield return c;

            for (var c = 'A'; c <= 'F'; c++)
                yield return c;

            for (var c = '0'; c <= '9'; c++)
                yield return c;
        }

        private static IEnumerable<char> EnumerateAllowedCharacters()
        {
            for (var c = 'a'; c <= 'z'; c++)
                yield return c;

            for (var c = 'A'; c <= 'Z'; c++)
                yield return c;

            for (var c = '0'; c <= '9'; c++)
                yield return c;

            yield return '.';
            yield return '-';
            yield return '_';
            yield return '~';
            yield return '*';
            yield return '(';
            yield return ')';
        }

        #endregion
    }
}