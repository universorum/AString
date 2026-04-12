using System.Buffers;
using System.Text;
using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringAppender
{
    [PublicAPI]
    public byte[] ToUtf8Array()
    {
        var        length = GetUft8ByteCount();
        Span<byte> span   = stackalloc byte[length];
        GetUft8Bytes(span);
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
    public int GetUft8ByteCount()
    {
        var       result     = 0;
        using var enumerator = GetChunks();
        while (enumerator.MoveNext()) { result += Encoding.UTF8.GetByteCount(enumerator.Current.Span); }

        return result;
    }

    [PublicAPI]
    public int GetUft8Bytes(Span<byte> buffer)
    {
        ArgumentException.ThrowIfTrue(buffer.Length < GetUft8ByteCount(), nameof(buffer));

        var       charsWritten = 0;
        using var enumerator   = GetChunks();
        while (enumerator.MoveNext())
        {
            var charSpan = enumerator.Current.Span;
            var length   = Encoding.UTF8.GetByteCount(charSpan);

            var localBuffer = ArrayPool<byte>.Shared.Rent(length);
            var span        = localBuffer.AsSpan();
            Encoding.UTF8.GetBytes(charSpan, span);
            span[..length].CopyTo(buffer);

            charsWritten += length;
            buffer       =  buffer[length..];
            ArrayPool<byte>.Shared.Return(localBuffer, CleanBufferWhenReleased);
        }

        return charsWritten;
    }

    [PublicAPI]
    public bool TryGetUft8Bytes(Span<byte> buffer, out int written)
    {
        written = 0;
        using var enumerator = GetChunks();
        while (enumerator.MoveNext())
        {
            var charSpan = enumerator.Current.Span;
            var length   = Encoding.UTF8.GetByteCount(charSpan);

            var localBuffer = ArrayPool<byte>.Shared.Rent(length);
            var span        = localBuffer.AsSpan();
            Encoding.UTF8.GetBytes(charSpan, span);
            if (buffer.Length < length) { return false; }

            span[..length].CopyTo(buffer);
            written += length;
            buffer  =  buffer[length..];
            ArrayPool<byte>.Shared.Return(localBuffer, CleanBufferWhenReleased);
        }

        return true;
    }

    [PublicAPI]
    public readonly void CopyUtf8To(int sourceIndex, Span<byte> destination, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentOutOfRangeException.ThrowIfNegative(sourceIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, destination.Length);

        using var enumerator = GetChunks(sourceIndex, count);
        while (enumerator.MoveNext())
        {
            var charSpan = enumerator.Current.Span;
            var length   = Encoding.UTF8.GetByteCount(charSpan);

            var buffer = ArrayPool<byte>.Shared.Rent(length);
            var span   = buffer.AsSpan();
            Encoding.UTF8.GetBytes(charSpan, span);
            span[..length].CopyTo(destination);
            destination = destination[length..];
            ArrayPool<byte>.Shared.Return(buffer, CleanBufferWhenReleased);
        }
    }

    [PublicAPI]
    public readonly void CopyUtf8To(int sourceIndex, byte[] destination, int destinationIndex, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(destinationIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(destinationIndex + count, destination.Length);

        var destSpan = destination.AsSpan(destinationIndex, count);

        using var enumerator = GetChunks(sourceIndex, count);
        while (enumerator.MoveNext())
        {
            var charSpan = enumerator.Current.Span;
            var length   = Encoding.UTF8.GetByteCount(charSpan);

            var buffer = ArrayPool<byte>.Shared.Rent(length);
            var span   = buffer.AsSpan();
            Encoding.UTF8.GetBytes(charSpan, span);
            span[..length].CopyTo(destSpan);
            destSpan = destSpan[length..];
            ArrayPool<byte>.Shared.Return(buffer, CleanBufferWhenReleased);
        }
    }


    [PublicAPI]
    public readonly bool TryCopyUtf8To(Span<byte> destination, out int charsWritten)
    {
        charsWritten = 0;

        using var enumerator = GetChunks();
        while (enumerator.MoveNext())
        {
            var charSpan = enumerator.Current.Span;
            var length   = Encoding.UTF8.GetByteCount(charSpan);

            if (destination.Length < length) { return false; }

            var buffer = ArrayPool<byte>.Shared.Rent(length);
            var span   = buffer.AsSpan();
            Encoding.UTF8.GetBytes(charSpan, span);
            span[..length].CopyTo(destination);
            destination  =  destination[length..];
            charsWritten += length;
            ArrayPool<byte>.Shared.Return(buffer, CleanBufferWhenReleased);
        }

        return true;
    }

    [PublicAPI]
    public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var enumerator = GetChunks();
        while (enumerator.MoveNext())
        {
            var charMemory = enumerator.Current;
            var length     = Encoding.UTF8.GetByteCount(charMemory.Span);
            var buffer     = ArrayPool<byte>.Shared.Rent(length);
            var memory     = buffer.AsMemory(0, length);
            Encoding.UTF8.GetBytes(charMemory.Span, memory.Span);

            await stream.WriteAsync(memory, cancellationToken).ConfigureAwait(false);

            ArrayPool<byte>.Shared.Return(buffer, CleanBufferWhenReleased);
        }
    }
}