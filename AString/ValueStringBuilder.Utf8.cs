using System.Buffers;
using System.Text;
using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringBuilder
{
    [PublicAPI]
    public byte[] ToUtf8Array()
    {
        var charSpan = AsSpan();
        var length   = Encoding.UTF8.GetByteCount(charSpan);
        if (length == 0) { return []; }

        Span<byte> span = stackalloc byte[length];
        Encoding.UTF8.GetBytes(charSpan, span);

        return span.ToArray();
    }

    [PublicAPI]
    public void AppendUft8(ReadOnlySpan<byte> uft8)
    {
        var size = Encoding.UTF8.GetCharCount(uft8);
        if (size == 0) { return; }

        Span<char> span = stackalloc char[size];
        Encoding.UTF8.GetChars(uft8, span);
        Append(span);
    }

    [PublicAPI]
    public void AppendUft8Line(ReadOnlySpan<byte> uft8)
    {
        AppendUft8(uft8);
        AppendLine();
    }

    [PublicAPI]
    public int GetUft8ByteCount() => Encoding.UTF8.GetByteCount(AsSpan());

    [PublicAPI]
    public int GetUft8Bytes(Span<byte> buffer)
    {
        ArgumentException.ThrowIfTrue(buffer.Length < GetUft8ByteCount(), nameof(buffer));

        return Encoding.UTF8.GetBytes(AsSpan(), buffer);
    }

    [PublicAPI]
    public bool TryGetUft8Bytes(Span<byte> buffer, out int written) =>
        Encoding.UTF8.TryGetBytes(AsSpan(), buffer, out written);

    [PublicAPI]
    public readonly void CopyUtf8To(int sourceIndex, Span<byte> destination, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        ArgumentOutOfRangeException.ThrowIfNegative(sourceIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, destination.Length);

        var charSpan = AsSpan();
        var length   = Encoding.UTF8.GetByteCount(charSpan);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sourceIndex,         length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sourceIndex + count, length);

        Span<byte> span = stackalloc byte[length];
        Encoding.UTF8.GetBytes(charSpan, span);
        span.Slice(sourceIndex, count).CopyTo(destination);
    }

    [PublicAPI]
    public readonly void CopyUtf8To(int sourceIndex, byte[] destination, int destinationIndex, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(destinationIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(destinationIndex + count, destination.Length);

        var charSpan = AsSpan();
        var length   = Encoding.UTF8.GetByteCount(charSpan);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sourceIndex,         length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sourceIndex + count, length);

        Span<byte> span = stackalloc byte[length];
        Encoding.UTF8.GetBytes(charSpan, span);
        span.Slice(sourceIndex, count).CopyTo(destination.AsSpan(destinationIndex));
    }

    [PublicAPI]
    public readonly bool TryCopyUtf8To(Span<byte> destination, out int charsWritten)
    {
        var charSpan = AsSpan();
        var length   = Encoding.UTF8.GetByteCount(charSpan);

        if (destination.Length < length)
        {
            charsWritten = 0;
            return false;
        }

        Span<byte> span = stackalloc byte[length];
        Encoding.UTF8.GetBytes(charSpan, span);
        span.CopyTo(destination);
        charsWritten = _length;
        return true;
    }

    [PublicAPI]
    public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var charSpan = AsSpan();
        var length   = Encoding.UTF8.GetByteCount(charSpan);
        if (length == 0) { return; }

        var buffer = ArrayPool<byte>.Shared.Rent(length);
        var memory = buffer.AsMemory(0, length);
        Encoding.UTF8.GetBytes(charSpan, memory.Span);

        await stream.WriteAsync(memory, cancellationToken).ConfigureAwait(false);

        ArrayPool<byte>.Shared.Return(buffer);
    }
}