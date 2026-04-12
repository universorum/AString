#if NET8_0_OR_GREATER
using System.Collections;
using System.Reflection;
using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringBuilder
{
    [PublicAPI]
    public readonly ValueStringBuilderTextElementEnumerator EnumerateTextElement()
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringBuilder));
        return new ValueStringBuilderTextElementEnumerator(AsMemory());
    }

    public struct ValueStringBuilderTextElementEnumerator : IEnumerator<ReadOnlyMemory<char>>,
        IEnumerable<ReadOnlyMemory<char>>
    {
        private delegate int GetGraphemeClusterLengthDelegateType(ReadOnlySpan<char> input);

        private static readonly GetGraphemeClusterLengthDelegateType GetGraphemeClusterLengthDelegate;

        static ValueStringBuilderTextElementEnumerator() =>
            GetGraphemeClusterLengthDelegate =
                typeof(string).Assembly.GetType("System.Text.Unicode.TextSegmentationUtility")!.GetMethod(
                    "GetLengthOfFirstUtf16ExtendedGraphemeCluster",
                    BindingFlags.Public | BindingFlags.Static)!.CreateDelegate<GetGraphemeClusterLengthDelegateType>();

        private readonly ReadOnlyMemory<char> _str;

        private int                  _currentTextElementOffset;
        private int                  _currentTextElementLength;
        private ReadOnlyMemory<char> _currentTextElementSubstr;

        internal ValueStringBuilderTextElementEnumerator(ReadOnlyMemory<char> str)
        {
            _str = str;

            Reset();
        }

        [PublicAPI]
        public bool MoveNext()
        {
            _currentTextElementSubstr = default; // clear any cached substr

            var newOffset = _currentTextElementOffset + _currentTextElementLength;
            _currentTextElementOffset = newOffset; // advance
            _currentTextElementLength =
                0; // prevent future calls to MoveNext() or get_Current from succeeding if we've hit end of data

            if (newOffset >= _str.Length)
            {
                return false; // reached the end of the data
            }

            _currentTextElementLength = GetGraphemeClusterLengthDelegate.Invoke(_str.Span[newOffset..]);
            return true;
        }

        [PublicAPI] public ReadOnlyMemory<char> Current => GetTextElement();

        private ReadOnlyMemory<char> GetTextElement()
        {
            // Returned the cached substr if we've already generated it.
            // Otherwise perform the substr operation now.

            var currentSubstr = _currentTextElementSubstr;
            if (!currentSubstr.IsEmpty) { return currentSubstr; }

            if (_currentTextElementOffset >= _str.Length)
            {
                throw new InvalidOperationException("Enumeration has either not started or has already finished.");
            }

            currentSubstr             = _str.Slice(_currentTextElementOffset, _currentTextElementLength);
            _currentTextElementSubstr = currentSubstr;

            return currentSubstr;
        }

        [PublicAPI]
        public int ElementIndex =>
            _currentTextElementOffset >= _str.Length
                ? throw new InvalidOperationException("Enumeration has either not started or has already finished.")
                : _currentTextElementOffset;

        [PublicAPI]
        public void Reset()
        {
            // These first two fields are set to intentionally out-of-range values.
            // They'll be fixed up once the enumerator starts.

            _currentTextElementOffset = _str.Length;
            _currentTextElementLength = -_str.Length;
            _currentTextElementSubstr = null;
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }

        IEnumerator<ReadOnlyMemory<char>> IEnumerable<ReadOnlyMemory<char>>.GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}
#endif