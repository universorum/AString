using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringBuilder
{
    /// <summary>Inserts a string 0 or more times into this builder at the specified position.</summary>
    /// <param name="index">The index to insert in this builder.</param>
    /// <param name="value">The string to insert.</param>
    /// <param name="count">The number of times to insert the string.</param>
    [PublicAPI]
    public void Insert(int index, string? value, int count) => Insert(index, value.AsSpan(), count);

    [PublicAPI]
    public void Insert(int index, ReadOnlySpan<char> value, int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _length);
        if (value.IsEmpty || count == 0) { return; }

        scoped ReadOnlySpan<char> s;
        if (count == 1) { s = value; }
        else
        {
            Span<char> buffer = stackalloc char[value.Length * count];
            for (var i = 0; i < buffer.Length; i += value.Length) { value.CopyTo(buffer[i..]); }

            s = buffer;
        }

        Insert(index, s);
    }

    [PublicAPI]
    public void Insert(int index, string? value) => Insert(index, value.AsSpan());

    [PublicAPI]
    public void Insert(int index, char[] value, int startIndex, int charCount)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _length);
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex, value.Length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(charCount,  value.Length - startIndex);

        if (charCount == 0) { return; }

        Insert(index, value.AsSpan(startIndex, charCount));
    }


    [PublicAPI]
    public void Insert(int index, char[] value) => Insert(index, value.AsSpan());

    [PublicAPI]
    public void Insert(int index, ReadOnlySpan<char> value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _length);

        if (value.IsEmpty) { return; }

        var diff = value.Length;
        EnsureCapacity(_length + diff);

        var span = _buffer.AsSpan();

        span[index.._length].CopyTo(span[(index + diff)..]);
        value.CopyTo(span[index..]);
        _length += diff;
    }

    [PublicAPI]
    public void Insert<T>(int index, T value)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(index, _length);

        var guestLength = GetGuestLength<T>();

        scoped ReadOnlySpan<char> ros;
        Span<char>                span = stackalloc char[guestLength];


        if (FormatterCache.TryFormat(value, span, out var charsWritten, null)) { ros = span[..charsWritten]; }
        else
        {
            span = stackalloc char[span.Length * 2];
            if (FormatterCache.TryFormat(value, span, out charsWritten, null)) { ros = span[..charsWritten]; }
            else
            {
                var str = FormatterCache.Format(value);
                ros          = str.AsSpan();
                charsWritten = str.Length;
            }
        }

        ros = ros[..charsWritten];

        Insert(index, ros);
    }
}