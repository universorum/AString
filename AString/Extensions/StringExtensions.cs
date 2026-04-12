using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System;

internal static class StringExtensions
{
    extension(string? self)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Create(ReadOnlySpan<char> span)
        {
#if NETSTANDARD2_1_OR_GREATER || NET8_0_OR_GREATER
            return new string(span);
#else
            return string.CreateFromSpan(span);
#endif
        }

#if !NET8_0_OR_GREATER
        public ReadOnlySpan<char> AsSpan(Range range)
        {
            if (self == null) { return default; }

            var (offset, length) = range.GetOffsetAndLength(self.Length);
            return self.AsSpan().Slice(offset, length);
        }
#endif
    }

    extension(ReadOnlySpan<char> self)
    {
#if NETCOREAPP3_0_OR_GREATER
        public bool TryGetRuneAt(int index, out Rune value)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, self.Length);
            ArgumentOutOfRangeException.ThrowIfNegative(index);

            // Get span at StringBuilder index
            Span<char> chars = [self[index], '\0'];

            if (index + 1 < self.Length) { chars[1] = self[index + 1]; }

            var status = Rune.DecodeFromUtf16(chars, out var result, out _);
            if (status is OperationStatus.Done)
            {
                value = result;
                return true;
            }

            value = default;
            return false;
        }
#endif
    }
}