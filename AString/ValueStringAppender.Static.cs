using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringAppender
{
    [Pure]
    [PublicAPI]
    public static CharBufferWriter CreateBufferWriter() => new();

    [Pure]
    [PublicAPI]
    public static Utf8BufferWriter CreateUtf8BufferWriter() => new();
}