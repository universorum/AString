using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace Astra.Text;

[DebuggerDisplay("{AsSpan()}")]
[PublicAPI]
public partial struct ValueStringBuilder : IDisposable
{
    public static int DefaultFixedSize { get; private set; }

    static ValueStringBuilder()
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
            DefaultFixedSize = BiggestPowerOf2((int)available);
        }
        else { DefaultFixedSize = defaultSize; }

        static int BiggestPowerOf2(int n)
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

    public static int SetDefaultSize(int size) => DefaultFixedSize = SmallestPowerOf2(size);

    private readonly int    _fixedSize;
    private          char[] _buffer;
    private          bool   _disposed;
    private          int    _length;

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
        readonly get => Math.Min(_buffer.Length, MaxCapacity);
        set
        {
            if (value <= _buffer.Length) { return; }

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

            return _buffer[index];
        }
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _length);

            _buffer[index] = value;
        }
    }

    /// <summary>Initializes a new instance of the <see cref="ValueStringBuilder" /> class.</summary>
    [PublicAPI]
    public ValueStringBuilder() : this(DefaultFixedSize) { }

    /// <summary>Initializes a new instance of the <see cref="StringBuilder" /> class.</summary>
    /// <param name="value">The initial contents of this builder.</param>
    /// <param name="sizeHint">The initial capacity of this builder.</param>
    [PublicAPI]
    public ValueStringBuilder(string? value, [NonNegativeValue] int? sizeHint = null) : this(sizeHint
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
    public ValueStringBuilder(string? value,
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
    public ValueStringBuilder([NonNegativeValue] int sizeHint, [NonNegativeValue] int maxCapacity = int.MaxValue)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sizeHint);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sizeHint, maxCapacity);

        MaxCapacity = maxCapacity;
        _fixedSize  = DefaultFixedSize;

        var size = sizeHint > _fixedSize ? SmallestPowerOf2(sizeHint) : _fixedSize;

        _buffer = ArrayPool<char>.Shared.Rent(size);
    }

    /// <summary>Determines if the contents of this builder are equal to the contents of <see cref="ReadOnlySpan{Char}" />.</summary>
    /// <param name="span">The <see cref="ReadOnlySpan{Char}" />.</param>
    [PublicAPI]
    public readonly bool Equals(ReadOnlySpan<char> span)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        return span.Length == _length && _buffer.AsSpan(0, _length).SequenceEqual(span);
    }

    /// <summary>Determines if the contents of this builder are equal to the contents of another builder.</summary>
    /// <param name="builder">The other builder.</param>
    [PublicAPI]
    public readonly bool Equals(ValueStringBuilder builder)
    {
        ObjectDisposedException.ThrowIf(_disposed,         typeof(ValueStringBuilder));
        ObjectDisposedException.ThrowIf(builder._disposed, typeof(ValueStringBuilder));

        return builder._length == _length
               && _buffer.AsSpan(0, _length).SequenceEqual(builder._buffer.AsSpan(0, _length));
    }

    /// <summary>Determines if the contents of this builder are equal to the contents of another builder.</summary>
    /// <param name="builder">The BCL Stringbuilder.</param>
    [PublicAPI]
    public readonly bool Equals(StringBuilder builder)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Length != _length) { return false; }

#if !NETCOREAPP3_0_OR_GREATER
        var str = builder.ToString();
        return Equals(str);
#else
        var span = _buffer.AsSpan(0, builder.Length);
        var chunks = builder.GetChunks();

        while (chunks.MoveNext())
        {
            var memory = chunks.Current;
            var size = Math.Min(span.Length, memory.Length);

            if (!memory.Span[..size].SequenceEqual(span[..size])) { return false; }

            span = span[size..];
        }

        return true;
#endif
    }

    [PublicAPI]
    public readonly ReadOnlySpan<char> AsSpan() => _buffer.AsSpan(0, _length);

    [PublicAPI]
    public readonly ReadOnlySpan<char> AsSpan(int startIndex, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(startIndex, _length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, _length - startIndex);

        return _buffer.AsSpan(startIndex, length);
    }

    [PublicAPI]
    public readonly ReadOnlyMemory<char> AsMemory() => _buffer.AsMemory(0, _length);

    [PublicAPI]
    public readonly ReadOnlyMemory<char> AsMemory(int startIndex, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(startIndex, _length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, _length - startIndex);

        return _buffer.AsMemory(startIndex, length);
    }

    /// <summary>Creates a string from a substring of this builder.</summary>
    [PublicAPI]
    public readonly override string ToString()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));

        return _length == 0 ? string.Empty : string.Create(_buffer.AsSpan(0, _length));
    }

    /// <summary>Creates a string from a substring of this builder.</summary>
    /// <param name="startIndex">The index to start in this builder.</param>
    /// <param name="length">The number of characters to read in this builder.</param>
    [PublicAPI]
    public readonly string ToString(int startIndex, int length)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, _length - startIndex);

        if (length == 0) { return string.Empty; }

        return startIndex == 0 && length == _length
            ? ToString()
            : string.Create(_buffer.AsSpan(0, _length).Slice(startIndex, length));
    }

    [PublicAPI]
    public void Clear() { _length = 0; }

    [PublicAPI]
    public readonly void CopyTo(int sourceIndex, Span<char> destination, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        ArgumentOutOfRangeException.ThrowIfNegative(sourceIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sourceIndex,         _length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sourceIndex + count, _length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count,               destination.Length);

        _buffer.AsSpan(sourceIndex, count).CopyTo(destination);
    }

    [PublicAPI]
    public readonly void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(destinationIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sourceIndex,              _length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sourceIndex      + count, _length);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(destinationIndex + count, destination.Length);

        _buffer.AsSpan(sourceIndex, count).CopyTo(destination.AsSpan(destinationIndex));
    }

    [PublicAPI]
    public readonly bool TryCopyTo(Span<char> destination, out int charsWritten)
    {
        if (destination.Length < _length)
        {
            charsWritten = 0;
            return false;
        }

        _buffer.AsSpan(0, _length).CopyTo(destination);
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
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(capacity, MaxCapacity);

        if (_buffer.Length >= capacity) { return _buffer.Length; }

        Grow(capacity);
        return Capacity;
    }

    private void Grow(int sizeHint)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sizeHint, MaxCapacity);
        ArgumentOutOfRangeException.ThrowIfNegative(sizeHint);

        if (sizeHint <= _buffer.Length) { return; }

        var newBuffer = ArrayPool<char>.Shared.Rent(SmallestPowerOf2(sizeHint));

        _buffer.AsSpan(0, _length).CopyTo(newBuffer);
        ArrayPool<char>.Shared.Return(_buffer, CleanBufferWhenReleased);
        _buffer = newBuffer;
    }

    /// <inheritdoc />
    [PublicAPI]
    public void Dispose()
    {
        if (_disposed) { return; }

        _disposed = true;
        ArrayPool<char>.Shared.Return(_buffer, CleanBufferWhenReleased);
    }

    private static int GetGuestLength<T>() =>
        typeof(T).IsValueType ? Math.Min(GuestStringLength, Unsafe.SizeOf<T>()) * 8 : GuestStringLength;

    private static int SmallestPowerOf2(int n)
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