#if NETCOREAPP3_0_OR_GREATER
using System.Collections;
using System.Text;
using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringBuilder
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
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _length);
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        return AsSpan().TryGetRuneAt(index, out value);
    }

    [PublicAPI]
    public readonly ValueStringBuilderRuneEnumerator EnumerateRunes()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        return new ValueStringBuilderRuneEnumerator(AsMemory());
    }

    [PublicAPI]
    public struct ValueStringBuilderRuneEnumerator : IEnumerator<Rune>, IEnumerable<Rune>
    {
        private readonly ReadOnlyMemory<char> _memory;
        private          Rune                 _current;
        private          int                  _nextIndex;

        internal ValueStringBuilderRuneEnumerator(ReadOnlyMemory<char> memory)
        {
            _memory = memory;
            _current = default;
            _nextIndex = 0;
        }

        /// <summary>Gets the <see cref="Rune" /> at the current position of the enumerator.</summary>
        [PublicAPI]
        public readonly Rune Current => _current;

        /// <summary>Returns the current enumerator instance.</summary>
        /// <returns>The current enumerator instance.</returns>
        public readonly ValueStringBuilderRuneEnumerator GetEnumerator() => this;

        /// <summary>Advances the enumerator to the next <see cref="Rune" /> of the builder.</summary>
        /// <returns>
        ///     <see langword="true" /> if the enumerator successfully advanced to the next item; <see langword="false" /> if
        ///     the end of the builder has been reached.
        /// </returns>
        [PublicAPI]
        public bool MoveNext()
        {
            if ((uint)_nextIndex >= _memory.Length)
            {
                // reached the end of the string
                _current = default;
                return false;
            }

            if (!_memory.Span.TryGetRuneAt(_nextIndex, out _current))
            {
                // replace invalid sequences with U+FFFD
                _current = Rune.ReplacementChar;
            }

            // In UTF-16 specifically, invalid sequences always have length 1, which is the same
            // length as the replacement character U+FFFD. This means that we can always bump the
            // next index by the current scalar's UTF-16 sequence length. This optimization is not
            // generally applicable; for example, enumerating scalars from UTF-8 cannot utilize
            // this same trick.

            _nextIndex += _current.Utf16SequenceLength;
            return true;
        }

        /// <summary>Gets the <see cref="Rune" /> at the current position of the enumerator.</summary>
        readonly object IEnumerator.Current => _current;

        /// <summary>Releases all resources used by the current <see cref="ValueStringBuilderRuneEnumerator" /> instance.</summary>
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

        /// <summary>Resets the current <see cref="ValueStringBuilderRuneEnumerator" /> instance to the beginning of the builder.</summary>
        void IEnumerator.Reset()
        {
            _current = default;
            _nextIndex = 0;
        }
    }
}

#endif