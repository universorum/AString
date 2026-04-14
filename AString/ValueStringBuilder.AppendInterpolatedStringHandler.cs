using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringBuilder
{
    /// <summary>Appends the specified interpolated string to this instance.</summary>
    /// <param name="handler">The interpolated string to append.</param>
    [PublicAPI]
    public void Append([InterpolatedStringHandlerArgument("")] ref AppendInterpolatedStringHandler handler)
    {
        using (var enumerator = handler.GetChunks())
        {
            while (enumerator.MoveNext()) { Append(enumerator.Current); }
        }

        handler.Dispose();
    }

    // ReSharper disable once EntityNameCapturedOnly.Global
    /// <summary>Appends the specified interpolated string to this instance.</summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="handler">The interpolated string to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    [PublicAPI]
    public void Append(IFormatProvider? provider,
        [InterpolatedStringHandlerArgument("", nameof(provider))] ref AppendInterpolatedStringHandler handler)
    {
        using (var enumerator = handler.GetChunks())
        {
            while (enumerator.MoveNext()) { Append(enumerator.Current); }
        }

        handler.Dispose();
    }

    /// <summary>
    ///     Provides a handler used by the language compiler to append interpolated strings into
    ///     <see cref="ValueStringBuilder" /> instances.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [InterpolatedStringHandler]
    [PublicAPI]
    public ref struct AppendInterpolatedStringHandler : IDisposable
    {
        private readonly IFormatProvider? _provider;

        // Why does `InterpolatedStringHandlerArgumentAttribute` does not support ref argument?
        private ValueStringAppender _stringBuilder;

        /// <summary>Creates a handler used to translate an interpolated string into a <see cref="string" />.</summary>
        /// <param name="literalLength">
        ///     The number of constant characters outside of interpolation expressions in the interpolated
        ///     string.
        /// </param>
        /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
        /// <param name="from">The appender we will copy the `CleanBufferWhenReleased` value from it</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <remarks>
        ///     This is intended to be called only by compiler-generated code. Arguments are not validated as they'd otherwise
        ///     be for members intended to be used directly.
        /// </remarks>
        public AppendInterpolatedStringHandler(int literalLength,
            int formattedCount,
            ValueStringBuilder from,
            IFormatProvider? provider = null)
        {
            _stringBuilder = new ValueStringAppender { CleanBufferWhenReleased = from.CleanBufferWhenReleased };
            _provider      = provider;
        }

        public ValueStringAppender.ReadOnlyMemoryEnumerator GetChunks() => _stringBuilder.GetChunks();

        public override string ToString() => _stringBuilder.ToString();

        /// <summary>Writes the specified string to the handler.</summary>
        /// <param name="value">The string to write.</param>
        public void AppendLiteral(string value) => _stringBuilder.Append(value);

        #region AppendFormatted

        #region AppendFormatted T

        public void AppendFormatted<T>(T value) => AppendFormatted(value, 0, null);

        public void AppendFormatted<T>(T value, string format) => AppendFormatted(value, 0, format);

        public void AppendFormatted<T>(T value, ReadOnlySpan<char> format) => AppendFormatted(value, 0, format);

        public void AppendFormatted<T>(T value, int alignment) => AppendFormatted(value, alignment, null);

        public void AppendFormatted<T>(T value, int alignment, string? format)
        {
            _stringBuilder.AppendFormatInternal(ref value, alignment, format.AsSpan(), format, _provider);
        }

        public void AppendFormatted<T>(T value, int alignment, ReadOnlySpan<char> format)
        {
            _stringBuilder.AppendFormatInternal(ref value, alignment, format, null, _provider);
        }

        #endregion

        #region AppendFormatted ReadOnlySpan<char>

        public void AppendFormatted(ReadOnlySpan<char> value) => _stringBuilder.Append(value);

        public void AppendFormatted(ReadOnlySpan<char> value, ReadOnlySpan<char> format) =>
            AppendFormatted(value, 0, format);

        public void AppendFormatted(ReadOnlySpan<char> value, int alignment) =>
            AppendFormatted(value, alignment, default);

        public void AppendFormatted(ReadOnlySpan<char> value, int alignment, ReadOnlySpan<char> format)
        {
            if (alignment == 0) { _stringBuilder.Append(value); }
            else
            {
                var leftAlign = false;
                if (alignment < 0)
                {
                    leftAlign = true;
                    alignment = -alignment;
                }

                var paddingRequired = alignment - value.Length;
                if (paddingRequired <= 0) { _stringBuilder.Append(value); }
                else if (leftAlign)
                {
                    _stringBuilder.Append(value);
                    _stringBuilder.Append(' ', paddingRequired);
                }
                else
                {
                    _stringBuilder.Append(' ', paddingRequired);
                    _stringBuilder.Append(value);
                }
            }
        }

        #endregion

        #endregion

        public void Dispose() { _stringBuilder.Dispose(); }
    }
}