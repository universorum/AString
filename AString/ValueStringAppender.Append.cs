using System.Buffers;
using JetBrains.Annotations;
#if NETCOREAPP3_0_OR_GREATER
using System.Text;
#endif

namespace Astra.Text;

public partial struct ValueStringAppender
{
    /// <summary>Appends a string to the end of this builder.</summary>
    /// <param name="value">The string to append.</param>
    [PublicAPI]
    public void Append(string? value) => Append(value.AsSpan());

    [PublicAPI]
    public void Append(char[]? value) => Append((ReadOnlySpan<char>)value.AsSpan());

    /// <summary>Appends part of a string to the end of this builder.</summary>
    /// <param name="value">The string to append.</param>
    /// <param name="startIndex">The index to start in <paramref name="value" />.</param>
    /// <param name="count">The number of characters to read in <paramref name="value" />.</param>
    [PublicAPI]
    public void Append(string? value, int startIndex, int count) => Append(value.AsSpan(startIndex, count));

    /// <summary>Appends a range of characters to the end of this builder.</summary>
    /// <param name="value">The characters to append.</param>
    /// <param name="startIndex">The index to start in <paramref name="value" />.</param>
    /// <param name="charCount">The number of characters to read in <paramref name="value" />.</param>
    [PublicAPI]
    public void Append(char[]? value, int startIndex, int charCount) =>
        Append((ReadOnlySpan<char>)value.AsSpan(startIndex, charCount));

    [PublicAPI]
    public void Append(char value) => Append([value]);

    /// <summary>Appends a character 0 or more times to the end of this builder.</summary>
    /// <param name="value">The character to append.</param>
    /// <param name="repeatCount">The number of times to append <paramref name="value" />.</param>
    [PublicAPI]
    public void Append(char value, int repeatCount)
    {
        if (repeatCount <= 0) { return; }

        const int stackAllocThreshold = 1024;

        if (repeatCount <= stackAllocThreshold) { Fill(ref this, stackalloc char[repeatCount], value); }
        else { UseArrayBuffer(ref this, value, repeatCount); }

        static void UseArrayBuffer(ref ValueStringAppender self, char value, int repeatCount)
        {
            var array = ArrayPool<char>.Shared.Rent(repeatCount);
            try { Fill(ref self, array.AsSpan(0, repeatCount), value); }
            finally { ArrayPool<char>.Shared.Return(array); }
        }

        static void Fill(ref ValueStringAppender self, Span<char> span, char value)
        {
            span.Fill(value);
            self.Append((ReadOnlySpan<char>)span);
        }
    }

    [PublicAPI]
    public void Append(Memory<char> value) => Append((ReadOnlySpan<char>)value.Span);

    [PublicAPI]
    public void Append(ReadOnlyMemory<char> value) => Append(value.Span);

    [PublicAPI]
    public void Append(Span<char> value) => Append((ReadOnlySpan<char>)value);

    [PublicAPI]
    public void Append(ReadOnlySpan<char> value)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        if (value.IsEmpty) { return; }

        var newLength = value.Length + _length;
        ArgumentException.ThrowIfTrue(newLength > MaxCapacity, nameof(value));

        Grow(newLength);

        while (!value.IsEmpty)
        {
            var currentChunk = _buffers[_length / _fixedSize]!;
            var offset       = _length % _fixedSize;
            var available    = _fixedSize - offset;
            var toCopy       = Math.Min(available, value.Length);

            value[..toCopy].CopyTo(currentChunk.AsSpan(offset));
            _length += toCopy;
            value   =  value[toCopy..];
        }
    }

    [PublicAPI]
    public void Append(ValueStringAppender builder)
    {
        ArgumentException.ThrowIfTrue(builder._disposed, nameof(builder));
        EnsureCapacity(_length + builder._length);

        using var enumerator = builder.GetChunks();

        while (enumerator.MoveNext()) { Append(enumerator.Current); }
    }


    [PublicAPI]
    public void Append<T>(T value)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
        const int stackAllocThreshold = 1024;

        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        if (FormatterCache.TryGetStringLength(value, out var length))
        {
            if (TryAppend(ref this, value, length)) { return; }

            throw new FormatException(); // TODO
        }

        const int maxRetry    = 2;
        var       i           = 0;
        var       guestLength = GetGuestLength<T>();

        while (true)
        {
            if (TryAppend(ref this, value, guestLength * (i + 1))) { return; }

            if (i++ >= maxRetry) { break; }
        }

        Append(FormatterCache.Format(value));


        static bool TryAppend(ref ValueStringAppender self, T value, int guestLength) =>
            guestLength <= stackAllocThreshold
                ? Fill(ref self, stackalloc char[guestLength], value)
                : UseArrayBuffer(ref self, value, guestLength);

        static bool UseArrayBuffer(ref ValueStringAppender self, T value, int length)
        {
            var array = ArrayPool<char>.Shared.Rent(length);
            try { return Fill(ref self, array.AsSpan(0, length), value); }
            finally { ArrayPool<char>.Shared.Return(array); }
        }

        static bool Fill(ref ValueStringAppender self, Span<char> span, T value)
        {
            if (!FormatterCache.TryFormat(value, span, out var charsWritten, null)) { return false; }

            self.Append(span[..charsWritten]);
            return true;
        }
    }

#if NETCOREAPP3_0_OR_GREATER
    [PublicAPI]
    public void Append(Rune value)
    {
        Span<char> valueChars = stackalloc char[value.Utf16SequenceLength];
        var        length     = value.EncodeToUtf16(valueChars);
        Append(valueChars[..length]);
    }
#endif
}