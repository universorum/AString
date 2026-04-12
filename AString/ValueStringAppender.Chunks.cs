using System.Collections;
using System.ComponentModel;
using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringAppender
{
    /// <summary>
    ///     GetChunks returns ReadOnlyMemoryEnumerator that follows the IEnumerable pattern and thus can be used in a C#
    ///     'foreach' statements to retrieve the data in the StringBuilder as chunks (ReadOnlyMemory) of characters.  An
    ///     example use is: foreach (ReadOnlyMemory&lt;char&gt; chunk in sb.GetChunks()) foreach (char c in chunk.Span) { /*
    ///     operation on c } It is undefined what happens if the StringBuilder is modified while the chunk enumeration is
    ///     incomplete.  StringBuilder is also not thread-safe, so operating on it with concurrent threads is illegal.  Finally
    ///     the ReadOnlyMemory chunks returned are NOT guaranteed to remain unchanged if the StringBuilder is modified, so do
    ///     not cache them for later use either.  This API's purpose is efficiently extracting the data of a CONSTANT
    ///     StringBuilder. Creating a ReadOnlySpan from a ReadOnlyMemory  (the .Span property) is expensive compared to the
    ///     fetching of the character, so create a local variable for the SPAN if you need to use it in a nested for statement.
    ///     For example foreach (ReadOnlyMemory&lt;char&gt; chunk in sb.GetChunks()) { var span = chunk.Span; for (int i = 0; i
    ///     &lt; span.Length; i++) { /* operation on span[i] */ } }
    /// </summary>
    [PublicAPI]
    public readonly ReadOnlyMemoryEnumerator GetChunks() => new(_fixedSize, _buffers, _length);

    [PublicAPI]
    public readonly ReadOnlyMemoryEnumerator GetChunks(int start, int end) =>
        new(_fixedSize, _buffers, _length, start, end);

    /// <summary>
    ///     ReadOnlyMemoryEnumerator supports both the IEnumerable and IEnumerator pattern so foreach works (see
    ///     GetChunks).  It needs to be public (so the compiler can use it when building a foreach statement) but users
    ///     typically don't use it explicitly. (which is why it is a nested type).
    /// </summary>
    [PublicAPI]
    public struct ReadOnlyMemoryEnumerator : IEnumerator<ReadOnlyMemory<char>>, IEnumerable<ReadOnlyMemory<char>>
    {
        private readonly int       _fixedSize;
        private readonly char[]?[] _buffers;
        private readonly int       _length;
        private readonly int       _start;
        private readonly int       _end;
        private          int       _current;

        internal ReadOnlyMemoryEnumerator(int fixedSize, char[]?[] buffer, int length, int start = 0, int? count = null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(start, length);

            if (count.HasValue) { ArgumentOutOfRangeException.ThrowIfGreaterThan(count.Value, length - start); }
            else { count = length - start; }

            _fixedSize = fixedSize;
            _buffers   = buffer;
            _length    = length;
            _start     = start;
            _end       = start + count.Value;
            _current   = start / _fixedSize * _fixedSize;
        }

        /// <inheritdoc />
        [PublicAPI]
        public ReadOnlyMemory<char> Current { get; private set; }

        /// <inheritdoc />
        [PublicAPI]
        public bool MoveNext()
        {
            if (_current >= _end) { return false; }

            var index  = _current / _fixedSize;
            var chars  = _buffers[index]!;
            var start  = _current < _start ? _start - _current : 0;
            var length = Math.Min(chars.Length - start, _end - _current - start);
            _current += _fixedSize;
            Current  =  chars.AsMemory(start, length);
            return true;
        }

        /// <inheritdoc />
        [PublicAPI]
        public void Reset() { _current = _start; }

        /// <inheritdoc />
        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerator<ReadOnlyMemory<char>> GetEnumerator() => this;
    }
}