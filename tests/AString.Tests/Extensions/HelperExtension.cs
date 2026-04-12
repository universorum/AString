// ReSharper disable once CheckNamespace

namespace System;

public static class StringExtension
{
    extension(string self)
    {
#if !NET8_0_OR_GREATER
        public void CopyTo(Span<char> destination) { self.AsSpan().CopyTo(destination); }
#endif
#if !NET10_0_OR_GREATER
        public static string Concat<T>(ReadOnlySpan<T> values) => string.Concat(values.ToArray());

        public static string Join<T>(char separator, ReadOnlySpan<T> values) =>
            string.Join(separator, values.ToArray());
#endif
    }
}

public static class NintExtension
{
    extension(nint self)
    {
#if !NET8_0_OR_GREATER
        public static nint MaxValue => int.MaxValue;
        public static nint MinValue => int.MinValue;
#endif
    }
}

public static class NuintExtension
{
    extension(nuint self)
    {
#if !NET8_0_OR_GREATER
        public static nuint MaxValue => uint.MaxValue;
        public static nuint MinValue => uint.MinValue;
#endif
    }
}