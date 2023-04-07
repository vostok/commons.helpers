
using JetBrains.Annotations;
#if NETCOREAPP3_1_OR_GREATER
using System.Runtime.CompilerServices;
using System;
#endif

namespace Vostok.Commons.Helpers.Spans;

#if NETCOREAPP3_1_OR_GREATER
[PublicAPI]
internal static class SpanSplitter
{
    //todo можно конечно принимать ReadOnlySpan<T> но тогда требуется и IEquatable<T> или компарер, для char будет хуже работать
    /// <summary>
    /// Split span by delimiters. Behaviour same as String.Split
    /// </summary>
    /// <param name="source"></param>
    /// <param name="separators">each char is independent separator!</param>
    /// <param name="removeEmptyEntries">true to skip zero-length results</param>
    /// <returns></returns>
    public static Enumerable Split(this ReadOnlySpan<char> source, ReadOnlySpan<char> separators,
        bool removeEmptyEntries = false)
    {
        return new Enumerable(source, separators, removeEmptyEntries);
    }

    public readonly ref struct Enumerable
    {
        private readonly ReadOnlySpan<char> source;
        private readonly ReadOnlySpan<char> separators;
        private readonly bool removeEmptyEntries;

        public Enumerable(ReadOnlySpan<char> source, ReadOnlySpan<char> separators, bool removeEmptyEntries)
        {
            this.source = source;
            this.separators = separators;
            this.removeEmptyEntries = removeEmptyEntries;
        }

        public Enumerator GetEnumerator() => new Enumerator(source, separators, removeEmptyEntries);
    }

    public ref struct Enumerator
    {
        private readonly ReadOnlySpan<char> source;
        private readonly ReadOnlySpan<char> separators;
        private readonly bool removeEmptyEntries;

        private int index;

        private int firstChar;
        private int state;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ReadOnlySpan<char> source, ReadOnlySpan<char> separators, bool removeEmptyEntries)
        {
            this.source = source;
            this.separators = separators;
            this.removeEmptyEntries = removeEmptyEntries;
            firstChar = 0;
            state = 0;
            index = 0;
            Current = ReadOnlySpan<char>.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (index < source.Length)
            {
                var charIsSeparator = separators.Contains(source[index]);
                if (TryProcessChar(charIsSeparator))
                    return true;
                index++;
            }

            if (TryProcessChar(true))
            {
                state = 2;
                return true;
            }

            if (Current.Length > 0)
                Current = Span<char>.Empty; //erase
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private bool TryProcessChar(bool charIsSeparator)
        {
            switch (state)
            {
                case 0: //char
                    if (charIsSeparator)
                    {
                        state = 1;
                        if (index > firstChar || !removeEmptyEntries)
                        {
                            Current = source.Slice(firstChar, index - firstChar);
                            index++;
                            return true;
                        }
                    }

                    break;
                case 1: //delimiter
                    if (!charIsSeparator)
                    {
                        firstChar = index;
                        state = 0;
                        break;
                    }

                    if (!removeEmptyEntries)
                    {
                        index++;
                        Current = Span<char>.Empty;
                        return true;
                    }

                    break;
            }

            return false;
        }


        public ReadOnlySpan<char> Current { get; private set; }
    }
}

#endif