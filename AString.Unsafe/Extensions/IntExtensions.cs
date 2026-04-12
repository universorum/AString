// ReSharper disable once CheckNamespace

namespace System;

internal static class IntExtensions
{
    extension(int self)
    {
#if !NETSTANDARD2_1_OR_GREATER
        public static int Parse(ReadOnlySpan<char> s) => int.Parse(s.ToString());
#endif
    }
}