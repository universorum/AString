using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace Astra.Text;

[PublicAPI]
public partial struct ValueStringAppender : IDisposable
{
    public static int DefaultFixedSize { get; private set; }

    static ValueStringAppender()
    {
        /*
         * In BCL implement SharedArrayPool, the size of new T[] will be 2 ^ n
         * We choose 32768 because it is the largest power of 2 that is less than LOHThreshold
         * 2 ^ 16 = 65536 < 65558 = 32768 * sizeof(char) + ObjectHeaderSize < LOHThreshold = 85000 < 2 ^ 17 =131072
         */
        const int defaultSize = 32768;

#if !NET8_0_OR_GREATER
        DefaultFixedSize = defaultSize;
#else
        const int objectHeader = 24;

        if (GC.GetConfigurationVariables().TryGetValue("LOHThreshold", out var boxedValue)
            && boxedValue is long unboxedValue)
        {
            var available = (unboxedValue - objectHeader) / sizeof(char);
            DefaultFixedSize = LargestPowerOf2((int)available);
        }
        else { DefaultFixedSize = defaultSize; }

        static int LargestPowerOf2(int n)
        {
            if (n <= 0) { return 0; }

            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;

            return (n + 1) >> 1;
        }
#endif
    }

    public static int SetDefaultSize(int size)
    {
        return DefaultFixedSize = SmallestPowerOf2(size);

        static int SmallestPowerOf2(int n)
        {
            if (n < 1) { return 1; }

            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return n + 1;
        }
    }

    private readonly int       _fixedSize;
    private          char[]?[] _buffers;
    private          int       _bufferLength;
    private          bool      _disposed;
    private          int       _length;

    private readonly int InternalCapacity => _bufferLength * _fixedSize;

    public static int GuestStringLength { get; set; } = 1024;

    /// <summary>Gets the maximum capacity of this instance.</summary>
    /// <returns>The maximum number of characters this instance can hold.</returns>
    [PublicAPI]
    public int MaxCapacity { get; }

    /// <summary>Gets or sets the length of the current <see cref="T:System.Text.StringBuilder" /> object.</summary>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    ///     The value specified for a set operation is less than zero or
    ///     greater than <see cref="P:System.Text.StringBuilder.MaxCapacity" />.
    /// </exception>
    /// <returns>The length of this instance.</returns>
    [PublicAPI]
    public int Length
    {
        get => _length;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxCapacity);

            EnsureCapacity(value);
            _length = value;
        }
    }

    // TODO
    [PublicAPI] public bool CleanBufferWhenReleased { get; set; }

    [PublicAPI]
    public int Capacity
    {
        readonly get => Math.Min(InternalCapacity, MaxCapacity);
        set
        {
            if (value <= _buffers.Length) { return; }

            Grow(value);
        }
    }

    [IndexerName("Chars")]
    [PublicAPI]
    public char this[int index]
    {
        readonly get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _length);

            var chunk  = index / _fixedSize;
            var offset = index % _fixedSize;
            return _buffers[chunk]![offset];
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _length);

            var chunk  = index / _fixedSize;
            var offset = index % _fixedSize;
            _buffers[chunk]![offset] = value;
        }
    }

    /// <summary>Initializes a new instance of the <see cref="ValueStringAppender" /> class.</summary>
    [PublicAPI]
    public ValueStringAppender() : this(DefaultFixedSize) { }

    /// <summary>Initializes a new instance of the <see cref="StringBuilder" /> class.</summary>
    /// <param name="value">The initial contents of this builder.</param>
    /// <param name="sizeHint">The initial capacity of this builder.</param>
    [PublicAPI]
    public ValueStringAppender(string? value, [NonNegativeValue] int? sizeHint = null) : this(sizeHint
        ?? DefaultFixedSize)
    {
        if (value == null) { return; }

        var span = value.AsSpan();
        Append(span);
    }

    /// <summary>Initializes a new instance of the <see cref="StringBuilder" /> class.</summary>
    /// <param name="value">The initial contents of this builder.</param>
    /// <param name="startIndex">The index to start in <paramref name="value" />.</param>
    /// <param name="length">The number of characters to read in <paramref name="value" />.</param>
    /// <param name="sizeHint">The initial capacity of this builder.</param>
    [PublicAPI]
    public ValueStringAppender(string? value,
        [NonNegativeValue] int startIndex,
        [NonNegativeValue] int length,
        [NonNegativeValue] int sizeHint) : this(sizeHint)
    {
        if (string.IsNullOrEmpty(value)) { return; }

        var span = value.AsSpan(startIndex, length);
        Append(span);
    }

    /// <summary>Initializes a new instance of the <see cref="StringBuilder" /> class.</summary>
    /// <param name="sizeHint">The initial capacity of this builder.</param>
    /// <param name="maxCapacity">The maximum capacity of this builder.</param>
    [PublicAPI]
    public ValueStringAppender([NonNegativeValue] int sizeHint, [NonNegativeValue] int maxCapacity = int.MaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sizeHint);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sizeHint, maxCapacity);

        _fixedSize  = DefaultFixedSize;
        MaxCapacity = maxCapacity;

        _buffers = ArrayPool<char[]?>.Shared.Rent(32);
        Grow(sizeHint);
    }

    /// <summary>Determines if the contents of this builder are equal to the contents of <see cref="ReadOnlySpan{Char}" />.</summary>
    /// <param name="span">The <see cref="ReadOnlySpan{Char}" />.</param>
    [PublicAPI]
    public readonly bool Equals(ReadOnlySpan<char> span)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        if (span.Length != _length) { return false; }

        var chunks = GetChunks();

        while (chunks.MoveNext())
        {
            var current = chunks.Current.Span;
            if (!current.SequenceEqual(span[..current.Length])) { return false; }

            span = span[current.Length..];
        }

        return true;
    }

    /// <summary>Determines if the contents of this builder are equal to the contents of another builder.</summary>
    /// <param name="builder">The other builder.</param>
    [PublicAPI]
    public readonly bool Equals(ValueStringAppender builder)
    {
        ObjectDisposedException.ThrowIf(_disposed,         typeof(ValueStringAppender));
        ObjectDisposedException.ThrowIf(builder._disposed, typeof(ValueStringAppender));

        if (builder._length != _length) { return false; }

        using var self  = GetChunks();
        var       other = builder.GetChunks();

        ReadOnlySpan<char> selfSpan  = default;
        ReadOnlySpan<char> otherSpan = default;

        while (true)
        {
            if (selfSpan.IsEmpty)
            {
                if (!self.MoveNext()) { break; }

                selfSpan = self.Current.Span;
            }

            if (otherSpan.IsEmpty)
            {
                if (!other.MoveNext()) { break; }

                otherSpan = other.Current.Span;
            }

            var len = Math.Min(selfSpan.Length, otherSpan.Length);
            if (!selfSpan[..len].SequenceEqual(otherSpan[..len])) { return false; }

            selfSpan  = selfSpan[len..];
            otherSpan = otherSpan[len..];
        }

        return selfSpan.IsEmpty && otherSpan.IsEmpty;
    }

    /// <summary>Determines if the contents of this builder are equal to the contents of another builder.</summary>
    /// <param name="builder">The BCL Stringbuilder.</param>
    [PublicAPI]
    public readonly bool Equals(StringBuilder builder)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Length != _length) { return false; }

#if NET8_0_OR_GREATER
        using var self = GetChunks();
        var       other = builder.GetChunks();

        ReadOnlySpan<char> selfSpan = default;
        ReadOnlySpan<char> otherSpan = default;

        while (true)
        {
            if (selfSpan.IsEmpty)
            {
                if (!self.MoveNext()) { break; }

                selfSpan = self.Current.Span;
            }

            if (otherSpan.IsEmpty)
            {
                if (!other.MoveNext()) { break; }

                otherSpan = other.Current.Span;
            }

            var len = Math.Min(selfSpan.Length, otherSpan.Length);
            if (!selfSpan[..len].SequenceEqual(otherSpan[..len])) { return false; }

            selfSpan = selfSpan[len..];
            otherSpan = otherSpan[len..];
        }

        return selfSpan.IsEmpty && otherSpan.IsEmpty;
#else
        var str = builder.ToString();
        return Equals(str);
#endif
    }

    /// <summary>Creates a string from a substring of this builder.</summary>
    [PublicAPI]
    public readonly override string ToString()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        if (_length < _fixedSize) { return string.Create(_buffers[0]!.AsSpan(0, _length)); }

        using var enumerator = GetChunks();

        return string.Create(_length,
            enumerator,
            static (buffer, enumerator) =>
            {
                while (enumerator.MoveNext())
                {
                    var span = enumerator.Current.Span;
                    span.CopyTo(buffer);
                    buffer = buffer[span.Length..];
                }
            });
    }

    /// <summary>Creates a string from a substring of this builder.</summary>
    /// <param name="startIndex">The index to start in this builder.</param>
    /// <param name="length">The number of characters to read in this builder.</param>
    [PublicAPI]
    public readonly string ToString(int startIndex, int length)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, _length - startIndex);

        if (length == 0) { return string.Empty; }

        var startChunk = startIndex / _fixedSize;
        if (startIndex + length <= (startChunk + 1) * _fixedSize)
        {
            var chunk  = _buffers[startChunk]!;
            var offset = startIndex % _fixedSize;

            return string.Create(chunk.AsSpan(offset, length));
        }

        using var enumerator = GetChunks(startIndex, length);
        return string.Create(length,
            enumerator,
            static (buffer, enumerator) =>
            {
                while (enumerator.MoveNext())
                {
                    var span = enumerator.Current.Span;
                    span.CopyTo(buffer);
                    buffer = buffer[span.Length..];
                }
            });
    }

    [PublicAPI]
    public void Clear() { _length = 0; }

    [PublicAPI]
    public readonly void CopyTo(int sourceIndex, Span<char> destination, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentOutOfRangeException.ThrowIfNegative(sourceIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sourceIndex,         _length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sourceIndex + count, _length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count,               destination.Length);

        using var enumerator = GetChunks(sourceIndex, count);

        while (enumerator.MoveNext())
        {
            var span = enumerator.Current.Span;
            span.CopyTo(destination);
            destination = destination[span.Length..];
        }
    }

    [PublicAPI]
    public readonly void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(destinationIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sourceIndex,              _length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sourceIndex      + count, _length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(destinationIndex + count, destination.Length);

        var destSpan = destination.AsSpan(destinationIndex);

        using var enumerator = GetChunks(sourceIndex, count);

        while (enumerator.MoveNext())
        {
            var span = enumerator.Current.Span;
            span.CopyTo(destSpan);
            destSpan = destSpan[span.Length..];
        }
    }

    [PublicAPI]
    public readonly bool TryCopyTo(Span<char> destination, out int charsWritten)
    {
        if (destination.Length < _length)
        {
            charsWritten = 0;
            return false;
        }

        using var enumerator = GetChunks();

        while (enumerator.MoveNext())
        {
            var span = enumerator.Current.Span;
            span.CopyTo(destination);
            destination = destination[span.Length..];
        }

        charsWritten = _length;
        return true;
    }

    /// <summary>Ensures that the capacity of this builder is at least the specified value.</summary>
    /// <param name="capacity">The new capacity for this builder.</param>
    /// <remarks>
    ///     If <paramref name="capacity" /> is less than or equal to the current capacity of this builder, the capacity
    ///     remains unchanged.
    /// </remarks>
    [PublicAPI]
    public int EnsureCapacity(int capacity)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(capacity, MaxCapacity);

        if (Capacity >= capacity) { return Capacity; }

        Grow(capacity);
        return Capacity;
    }

    private void Grow(int sizeHint)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sizeHint, MaxCapacity);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);

        if (sizeHint <= InternalCapacity) { return; }

        var diff  = sizeHint - InternalCapacity;
        var count = (int)Math.Ceiling((double)diff / _fixedSize);

        if (count + _bufferLength > _buffers.Length)
        {
            var newBuffer = ArrayPool<char[]?>.Shared.Rent(count + _bufferLength);

            _buffers.AsSpan(0, _bufferLength).CopyTo(newBuffer);
            ArrayPool<char[]?>.Shared.Return(_buffers, CleanBufferWhenReleased);
            _buffers = newBuffer;
        }

        while (count > 0)
        {
            var chars = ArrayPool<char>.Shared.Rent(_fixedSize);

            _buffers[_bufferLength++] = chars;
            count--;
        }
    }

    /// <inheritdoc />
    [PublicAPI]
    public void Dispose()
    {
        if (_disposed) { return; }

        _disposed = true;
        for (var i = 0; i < _bufferLength; i++)
        {
            ArrayPool<char>.Shared.Return(_buffers[i]!, CleanBufferWhenReleased);
        }

        ArrayPool<char[]?>.Shared.Return(_buffers, CleanBufferWhenReleased);
    }

    private static int GetGuestLength<T>() =>
        typeof(T).IsValueType ? Math.Min(GuestStringLength, Unsafe.SizeOf<T>()) * 8 : GuestStringLength;
}