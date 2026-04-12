using System.Buffers;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
#if NETCOREAPP3_0_OR_GREATER
using System.Text;
#endif

namespace Astra.Text;

public partial struct ValueStringAppender
{
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

    // /// <summary>Appends a string to the end of this builder.</summary>
    // /// <param name="value">The string to append.</param>
    // public void Append(string? value) => Append(value.AsSpan());

    /// <summary>Appends part of a string to the end of this builder.</summary>
    /// <param name="value">The string to append.</param>
    /// <param name="startIndex">The index to start in <paramref name="value" />.</param>
    /// <param name="count">The number of characters to read in <paramref name="value" />.</param>
    [PublicAPI]
    public void Append(string? value, int startIndex, int count) => Append(value.AsSpan(startIndex, count));

    [PublicAPI]
    public void Append(ReadOnlyMemory<char> value) => Append(value.Span);

    [PublicAPI]
    public void Append(char[]? value)
    {
        if (value is not { Length: > 0 }) { return; }

        Append(value.AsSpan());
    }

    /// <summary>Appends a range of characters to the end of this builder.</summary>
    /// <param name="value">The characters to append.</param>
    /// <param name="startIndex">The index to start in <paramref name="value" />.</param>
    /// <param name="charCount">The number of characters to read in <paramref name="value" />.</param>
    [PublicAPI]
    public void Append(char[]? value, int startIndex, int charCount)
    {
        if (value is not { Length: > 0 }) { return; }

        Append(value.AsSpan(startIndex, charCount));
    }

    [PublicAPI]
    public void Append(char value)
    {
        Span<char> s = [value];
        Append(s);
    }

    /// <summary>Appends a character 0 or more times to the end of this builder.</summary>
    /// <param name="value">The character to append.</param>
    /// <param name="repeatCount">The number of times to append <paramref name="value" />.</param>
    [PublicAPI]
    public void Append(char value, int repeatCount)
    {
        if (repeatCount <= 0) { return; }

        const int stackAllocThreshold = 1024;
        if (repeatCount <= stackAllocThreshold)
        {
            Span<char> s = stackalloc char[repeatCount];
            s.Fill(value);
            Append(s);
        }
        else
        {
            var array = ArrayPool<char>.Shared.Rent(repeatCount);
            try
            {
                var span = array.AsSpan(0, repeatCount);
                span.Fill(value);
                Append(span);
            }
            finally { ArrayPool<char>.Shared.Return(array); }
        }
    }

    [PublicAPI]
    public void Append<T>(T value)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        if (typeof(T) == typeof(string))
        {
            Append(Unsafe.As<string>(value).AsSpan());
            return;
        }

        const int minimumPredictedCommonLength = 2;
        EnsureCapacity(_length + minimumPredictedCommonLength);

        const int maxRetry    = 2;
        var       i           = 0;
        var       guestLength = GetGuestLength<T>();

        while (true)
        {
            if (TryAppend(ref this, value, guestLength * (i + 1))) { return; }

            if (i++ >= maxRetry) { break; }
        }

        var str = FormatterCache.Format(value, null);
        Append(str);

        static bool TryAppend(ref ValueStringAppender self, T value, int guestLength)
        {
            Span<char> buffer = stackalloc char[guestLength];
            if (!FormatterCache.TryFormat(value, buffer, out var charsWritten, null)) { return false; }

            self.Append(buffer[..charsWritten]);
            return true;
        }
    }

#if NETCOREAPP3_0_OR_GREATER
    [PublicAPI]
    public void Append(Rune value)
    {
        Span<char> valueChars = stackalloc char[value.Utf16SequenceLength];
        var        length = value.EncodeToUtf16(valueChars);
        Append(valueChars[..length]);
    }
#endif
}