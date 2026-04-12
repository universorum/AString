using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringAppender
{
    /// <summary>Removes a range of characters from this builder.</summary>
    /// <remarks>This method does not reduce the capacity of this builder.</remarks>
    [PublicAPI]
    public void Remove(int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, _length);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        _length -= length;
    }
}