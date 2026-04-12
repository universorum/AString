using System.Buffers;
using JetBrains.Annotations;

namespace Astra.Text;

[PublicAPI]
public struct Utf8BufferWriter() : IBufferWriter<byte>, IDisposable
{
    private ValueStringBuilder _builder = new();
    private byte[]?            _buffer;
    private bool               _disposed;

    [PublicAPI] public bool CleanBufferWhenReleased { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(CharBufferWriter));
        return _builder.ToString();
    }

    [PublicAPI]
    public string ToString(int startIndex, int length)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(CharBufferWriter));
        return _builder.ToString(startIndex, length);
    }

    [PublicAPI]
    public readonly bool TryCopyCharTo(Span<char> destination, out int charsWritten)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(CharBufferWriter));
        return _builder.TryCopyTo(destination, out charsWritten);
    }

    [PublicAPI]
    public readonly bool TryCopyTo(Span<byte> destination, out int charsWritten)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(CharBufferWriter));
        return _builder.TryCopyUtf8To(destination, out charsWritten);
    }

    [PublicAPI]
    public readonly void AppendTo(ref ValueStringBuilder builder)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(CharBufferWriter));
        builder.Append(builder.AsSpan());
    }

    [PublicAPI]
    public readonly void AppendTo(ref ValueStringBuilder builder, int startIndex, int length)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(CharBufferWriter));
        builder.Append(builder.AsSpan(startIndex, length));
    }

    /// <inheritdoc />
    [PublicAPI]
    public void Advance(int count)
    {
        if (count == 0) { return; }

        ObjectDisposedException.ThrowIf(_disposed, typeof(CharBufferWriter));
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, _buffer?.Length ?? -1);

        var span = _buffer.AsSpan();
        _builder.AppendUft8(span[..count]);

        ArrayPool<byte>.Shared.Return(_buffer!, CleanBufferWhenReleased);
        _buffer = null;
    }

    /// <inheritdoc />
    [Pure]
    [PublicAPI]
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(CharBufferWriter));
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);

        if (_buffer != null)
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            if (_buffer.Length >= sizeHint) { return _buffer.AsMemory(); }

            ArrayPool<byte>.Shared.Return(_buffer, CleanBufferWhenReleased);
        }

        _buffer = ArrayPool<byte>.Shared.Rent(sizeHint);
        Array.Clear(_buffer, 0, _buffer.Length);
        return _buffer.AsMemory();
    }

    /// <inheritdoc />
    [Pure]
    [PublicAPI]
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(CharBufferWriter));
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);

        if (_buffer != null)
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            if (_buffer.Length >= sizeHint) { return _buffer.AsSpan(); }

            ArrayPool<byte>.Shared.Return(_buffer, CleanBufferWhenReleased);
        }

        _buffer = ArrayPool<byte>.Shared.Rent(sizeHint);
        Array.Clear(_buffer, 0, _buffer.Length);
        return _buffer.AsSpan();
    }

    /// <inheritdoc />
    [PublicAPI]
    public void Dispose()
    {
        _disposed = true;

        _builder.Dispose();
        if (_buffer != null) { ArrayPool<byte>.Shared.Return(_buffer, CleanBufferWhenReleased); }
    }
}