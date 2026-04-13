using System.Buffers;
using JetBrains.Annotations;
#if NETCOREAPP3_0_OR_GREATER
using System.Text;
#endif

namespace Astra.Text;

public partial struct ValueStringBuilder
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

        static void UseArrayBuffer(ref ValueStringBuilder self, char value, int repeatCount)
        {
            var array = ArrayPool<char>.Shared.Rent(repeatCount);
            try { Fill(ref self, array.AsSpan(0, repeatCount), value); }
            finally { ArrayPool<char>.Shared.Return(array); }
        }

        static void Fill(ref ValueStringBuilder self, Span<char> span, char value)
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
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));

        if (value.IsEmpty) { return; }

        var newLength = value.Length + _length;
        ArgumentException.ThrowIfTrue(newLength > MaxCapacity, nameof(value));

        Grow(newLength);

        value.CopyTo(_buffer.AsSpan(_length));
        _length += value.Length;
    }

    [PublicAPI]
    public void Append(ValueStringBuilder builder)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        ArgumentException.ThrowIfTrue(builder._disposed, nameof(builder));
        EnsureCapacity(_length + builder._length);

        Append(builder.AsSpan());
    }

    [PublicAPI]
    public void Append(ValueStringAppender appender)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        EnsureCapacity(_length + appender.Length);

        var chunks = appender.GetChunks();

        while (chunks.MoveNext()) { Append(chunks.Current); }
    }


    [PublicAPI]
    public void Append<T>(T value)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));

        if (FormatterCache.TryGetStringLength(value, out var length))
        {
            EnsureCapacity(_length + length);
            if (!FormatterCache.TryFormat(value, _buffer.AsSpan(_length), out var charsWritten, null))
            {
                throw new FormatException(); // TODO
            }

            _length += charsWritten;
            return;
        }

        // const int minimumPredictedCommonLength = 2;
        // EnsureCapacity(_length + minimumPredictedCommonLength);

        const int maxRetry    = 2;
        var       i           = 0;
        var       guestLength = GetGuestLength<T>();

        while (true)
        {
            EnsureCapacity(_length + guestLength * (i + 1));

            if (FormatterCache.TryFormat(value, _buffer.AsSpan(_length), out var charsWritten, null))
            {
                _length += charsWritten;
                return;
            }

            if (i++ >= maxRetry) { break; }
        }

        var str = FormatterCache.Format(value);
        Append(str);
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