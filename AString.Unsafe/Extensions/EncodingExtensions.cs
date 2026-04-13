using System.Text;

// ReSharper disable once CheckNamespace
namespace System;

internal static class EncodingExtensions
{
    extension(Encoding self)
    {
        public bool TryGetBytes(ReadOnlySpan<char> chars, Span<byte> bytes, out int bytesWritten)
        {
            var required = self.GetByteCount(chars);
            if (required <= bytes.Length)
            {
                bytesWritten = self.GetBytes(chars, bytes);
                return true;
            }

            bytesWritten = 0;
            return false;
        }

        public int GetByteCount(ReadOnlySpan<char> span)
        {
            if (span.IsEmpty) { return 0; }

            unsafe
            {
                fixed (char* src = span) { return self.GetByteCount(src, span.Length); }
            }
        }

#if !NETSTANDARD2_1_OR_GREATER
        public int GetBytes(ReadOnlySpan<char> span, Span<byte> bytes)
        {
            unsafe
            {
                fixed (char* src = span)
                fixed (byte* dest = bytes) { return self.GetBytes(src, span.Length, dest, bytes.Length); }
            }
        }

        public int GetCharCount(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty) { return 0; }

            unsafe
            {
                fixed (byte* src = span) { return self.GetCharCount(src, span.Length); }
            }
        }

        public void GetChars(ReadOnlySpan<byte> bytes, Span<char> chars)
        {
            unsafe
            {
                fixed (byte* src = bytes)
                fixed (char* dest = chars) { self.GetChars(src, bytes.Length, dest, chars.Length); }
            }
        }
#endif
    }
}