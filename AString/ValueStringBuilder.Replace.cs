using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringBuilder
{
    /// <summary>Replaces all instances of one character with another in this builder.</summary>
    /// <param name="oldChar">The character to replace.</param>
    /// <param name="newChar">The character to replace <paramref name="oldChar" /> with.</param>
    [PublicAPI]
    public void Replace(char oldChar, char newChar) => Replace(oldChar, newChar, 0, _length);

    /// <summary>Replaces all instances of one character with another in this builder.</summary>
    /// <param name="oldChar">The character to replace.</param>
    /// <param name="newChar">The character to replace <paramref name="oldChar" /> with.</param>
    /// <param name="startIndex">The index to start in this builder.</param>
    /// <param name="count">The number of characters to read in this builder.</param>
    [PublicAPI]
    public void Replace(char oldChar, char newChar, int startIndex, int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex, _length);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex, _length - count);

        _buffer.AsSpan(startIndex, count).Replace(oldChar, newChar);
    }

    /// <summary>Replaces all instances of one read-only character span with another in this builder.</summary>
    /// <param name="oldValue">The read-only character span to replace.</param>
    /// <param name="newValue">The read-only character span to replace <paramref name="oldValue" /> with.</param>
    /// <remarks>
    ///     If <paramref name="newValue" /> is empty, instances of <paramref name="oldValue" /> are removed from this
    ///     builder.
    /// </remarks>
    [PublicAPI]
    public void Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue) =>
        Replace(oldValue, newValue, 0, _length);

    /// <summary>Replaces all instances of one read-only character span with another in part of this builder.</summary>
    /// <param name="oldValue">The read-only character span to replace.</param>
    /// <param name="newValue">The read-only character span to replace <paramref name="oldValue" /> with.</param>
    /// <param name="startIndex">The index to start in this builder.</param>
    /// <param name="count">The number of characters to read in this builder.</param>
    /// <remarks>
    ///     If <paramref name="newValue" /> is empty, instances of <paramref name="oldValue" /> are removed from this
    ///     builder.
    /// </remarks>
    [PublicAPI]
    public void Replace(ReadOnlySpan<char> oldValue, ReadOnlySpan<char> newValue, int startIndex, int count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex, _length);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex, _length - count);
        ArgumentException.ThrowIfEmpty(oldValue);

        scoped var span = _buffer.AsSpan(startIndex);

        var diff      = newValue.Length - oldValue.Length;
        var processed = 0;
        while (processed <= span.Length)
        {
            var i = span[processed..].IndexOf(oldValue);
            if (i < 0) { break; }

            processed += i;

            if (processed + oldValue.Length > count) { break; }

            if (diff == 0) { newValue.CopyTo(span.Slice(processed, newValue.Length)); }
            else
            {
                if (diff > 0)
                {
                    EnsureCapacity(_length + diff);
                    span = _buffer.AsSpan(startIndex);
                }

                var remainingLength = _length - startIndex - processed - oldValue.Length;
                span.Slice(processed + oldValue.Length, remainingLength)
                    .CopyTo(span.Slice(processed + newValue.Length, remainingLength));
                newValue.CopyTo(span[processed..]);
            }

            _length   += diff;
            count     += diff;
            processed += newValue.Length;
        }
    }
}