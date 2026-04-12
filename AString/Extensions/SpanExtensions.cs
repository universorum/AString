// ReSharper disable once CheckNamespace

namespace System;

public static class SpanExtensions
{
    extension<T>(Span<T> self) where T : IEquatable<T>
    {
#if !NET8_0_OR_GREATER
        public void Replace(T oldValue, T newValue)
        {
            for (var i = 0; i < self.Length; i++)
            {
                if (self[i].Equals(oldValue)) { self[i] = newValue; }
            }
        }
#endif
    }
}