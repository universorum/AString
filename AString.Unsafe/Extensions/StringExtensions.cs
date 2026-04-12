// ReSharper disable once CheckNamespace

using System.Buffers;

namespace System;

internal delegate void SpanAction<T, in TState>(Span<T> span, TState state);

internal static class StringExtensions
{
    extension(string self)
    {
#if !NETSTANDARD2_1_OR_GREATER
        public static string CreateFromSpan(ReadOnlySpan<char> charSpan)
        {
            unsafe
            {
                fixed (char* src = charSpan) { return new string(src, 0, charSpan.Length); }
            }
        }

        public static string Create<T>(int length, T state, SpanAction<char, T> action)
        {
            var array = ArrayPool<char>.Shared.Rent(length);
            try
            {
                var span = array.AsSpan(0, length);
                action(span, state);

                unsafe
                {
                    fixed (char* src = span)
                    {
                        return new string(src, 0, span.Length);
                    }
                }
            }
            finally { ArrayPool<char>.Shared.Return(array); }
        }
#endif
    }
}