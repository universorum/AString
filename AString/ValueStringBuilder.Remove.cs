using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringBuilder
{
    /// <summary>Removes a range of characters from this builder.</summary>
    /// <remarks>This method does not reduce the capacity of this builder.</remarks>
    [PublicAPI]
    public void Remove(int startIndex, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startIndex, _length);
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        var span = _buffer.AsSpan(startIndex);
        var end  = Math.Min(length, span.Length);
        span[end..].CopyTo(span);
        _length -= end;
    }
}