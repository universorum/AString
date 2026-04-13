#if NETCOREAPP3_0_OR_GREATER
using System.Collections;
using System.Text;
using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringAppender
{
    [PublicAPI]
    public readonly Rune GetRuneAt(int index) =>
        TryGetRuneAt(index, out var rune)
            ? rune
            : throw new ArgumentException(
                "Cannot extract a Unicode scalar value from the specified index in the input.",
                nameof(index));

    [PublicAPI]
    public readonly bool TryGetRuneAt(int index, out Rune value)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _length);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        var chunk  = index / _fixedSize;
        var offset = index % _fixedSize;
        var c1     = _buffers[chunk]![offset];

        Span<char> chars = [c1, '\0'];
        if (index + 1 < _length)
        {
            chars[1] = offset + 1 < _fixedSize ? _buffers[chunk]![offset + 1] : _buffers[chunk + 1]![0];
        }

        return chars.TryGetRuneAt(0, out value);
    }

    [PublicAPI]
    public readonly ValueStringAppenderRuneEnumerator EnumerateRunes()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        return new ValueStringAppenderRuneEnumerator(_fixedSize, _buffers, _length);
    }

    [PublicAPI]
    public struct ValueStringAppenderRuneEnumerator : IEnumerator<Rune>, IEnumerable<Rune>
    {
        private readonly int       _fixedSize;
        private readonly int       _length;
        private          char[]?[] _buffers;
        private          Rune      _current;
        private          int       _nextIndex;

        internal ValueStringAppenderRuneEnumerator(int fixedSize, char[]?[] buffers, int length)
        {
            _fixedSize = fixedSize;
            _buffers   = buffers;
            _length    = length;
            _current   = default;
            _nextIndex = 0;
        }

        /// <summary>Gets the <see cref="Rune" /> at the current position of the enumerator.</summary>
        [PublicAPI]
        public readonly Rune Current => _current;

        /// <summary>Returns the current enumerator instance.</summary>
        /// <returns>The current enumerator instance.</returns>
        public readonly ValueStringAppenderRuneEnumerator GetEnumerator() => this;

        /// <summary>Advances the enumerator to the next <see cref="Rune" /> of the builder.</summary>
        /// <returns>
        ///     <see langword="true" /> if the enumerator successfully advanced to the next item; <see langword="false" /> if
        ///     the end of the builder has been reached.
        /// </returns>
        [PublicAPI]
        public bool MoveNext()
        {
            if (_nextIndex >= _length) { return false; }

            var chunk  = _nextIndex / _fixedSize;
            var offset = _nextIndex % _fixedSize;
            var c1     = _buffers[chunk]![offset];

            Span<char> chars = [c1, '\0'];
            if (_nextIndex + 1 < _length)
            {
                chars[1] = offset + 1 < _fixedSize ? _buffers[chunk]![offset + 1] : _buffers[chunk + 1]![0];
            }

            if (!chars.TryGetRuneAt(0, out var rune)) { rune = Rune.ReplacementChar; }

            _current   =  rune;
            _nextIndex += rune.Utf16SequenceLength;
            return true;
        }

        /// <summary>Gets the <see cref="Rune" /> at the current position of the enumerator.</summary>
        readonly object IEnumerator.Current => _current;

        /// <summary>Releases all resources used by the current <see cref="ValueStringAppenderRuneEnumerator" /> instance.</summary>
        /// <remarks>This method performs no operation and produces no side effects.</remarks>
        readonly void IDisposable.Dispose()
        {
            // no-op
        }

        /// <summary>Returns the current enumerator instance.</summary>
        /// <returns>The current enumerator instance.</returns>
        readonly IEnumerator IEnumerable.GetEnumerator() => this;

        /// <summary>Returns the current enumerator instance.</summary>
        /// <returns>The current enumerator instance.</returns>
        readonly IEnumerator<Rune> IEnumerable<Rune>.GetEnumerator() => this;

        /// <summary>Resets the current <see cref="ValueStringAppenderRuneEnumerator" /> instance to the beginning of the builder.</summary>
        void IEnumerator.Reset()
        {
            _current   = default;
            _nextIndex = 0;
        }
    }
}

#endif