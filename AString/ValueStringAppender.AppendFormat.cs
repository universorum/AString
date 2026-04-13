using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Astra.Text.Models;
using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringAppender
{
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TArg0, TArg1, TArg2>(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0,
        TArg1 arg1,
        TArg2 arg2) =>
        AppendFormat(null, format, arg0, arg1, arg2);

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TArg0, TArg1>(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0,
        TArg1 arg1) =>
        AppendFormat(null, format, arg0, arg1);

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TArg0>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0) =>
        AppendFormat(null, format, arg0);

    /// <summary>
    ///     Appends the string returned by processing a composite format string, which contains zero or more format items,
    ///     to this instance. Each format item is replaced by the string representation of a corresponding argument in a
    ///     parameter span.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">A span of objects to format.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The length of the expanded string would exceed
    ///     <see cref="ValueStringAppender.MaxCapacity" />.
    /// </exception>
    /// <exception cref="FormatException">
    ///     <paramref name="format" /> is invalid. -or- The index of a format item is less than 0
    ///     (zero), or greater than or equal to the length of the <paramref name="args" /> span.
    /// </exception>
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<T>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        ReadOnlySpan<T?> args) =>
        AppendFormat(null, format, args);

    /// <summary>
    ///     Appends the string returned by processing a composite format string, which contains zero or more format items,
    ///     to this instance. Each format item is replaced by the string representation of a corresponding argument in a
    ///     parameter span using a specified format provider.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">A span of objects to format.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The length of the expanded string would exceed
    ///     <see cref="ValueStringAppender.MaxCapacity" />.
    /// </exception>
    /// <exception cref="FormatException">
    ///     <paramref name="format" /> is invalid. -or- The index of a format item is less than 0
    ///     (zero), or greater than or equal to the length of the <paramref name="args" /> span.
    /// </exception>
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<T>(IFormatProvider? provider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        ReadOnlySpan<T?> args)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var enumerator = new FormatSegmentEnumerator(format);

        while (enumerator.MoveNext()) { AppendSegment(enumerator.Current, format, provider, args); }
    }

    /// <summary>
    ///     Appends the string returned by processing a composite format string, which contains zero or more format items,
    ///     to this instance. Each format item is replaced by the string representation of any of the arguments using a
    ///     specified format provider.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="AStringCompositeFormat" />.</param>
    /// <param name="args">A span of objects to format.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
    /// <exception cref="FormatException">
    ///     The index of a format item is greater than or equal to the number of supplied
    ///     arguments.
    /// </exception>
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<T>(IFormatProvider? provider, AStringCompositeFormat format, ReadOnlySpan<T?> args)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(format);
        ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, format.MinimumArgumentCount);

        foreach (var segment in format.Segments) { AppendSegment(segment, format.Format, provider, args); }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TArg0, TArg1, TArg2>(IFormatProvider? provider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0,
        TArg1 arg1,
        TArg2 arg2)
#if NET9_0_OR_GREATER
        where TArg0 : allows ref struct where TArg1 : allows ref struct where TArg2 : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var enumerator = new FormatSegmentEnumerator(format);

        while (enumerator.MoveNext()) { AppendSegment(enumerator.Current, format, provider, 3, arg0, arg1, arg2); }
    }

    /// <summary>
    ///     Appends the string returned by processing a composite format string, which contains zero or more format items,
    ///     to this instance. Each format item is replaced by the string representation of any of the arguments using a
    ///     specified format provider.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <typeparam name="TArg1">The type of the second object to format.</typeparam>
    /// <typeparam name="TArg2">The type of the third object to format.</typeparam>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="AStringCompositeFormat" />.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <param name="arg2">The third object to format.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
    /// <exception cref="FormatException">
    ///     The index of a format item is greater than or equal to the number of supplied
    ///     arguments.
    /// </exception>
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TArg0, TArg1, TArg2>(IFormatProvider? provider,
        AStringCompositeFormat format,
        TArg0 arg0,
        TArg1 arg1,
        TArg2 arg2)
#if NET9_0_OR_GREATER
        where TArg0 : allows ref struct where TArg1 : allows ref struct where TArg2 : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(format);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(format.MinimumArgumentCount, 3);

        foreach (var segment in format.Segments)
        {
            AppendSegment(segment, format.Format, provider, 3, arg0, arg1, arg2);
        }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TArg0, TArg1>(IFormatProvider? provider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0,
        TArg1 arg1)
#if NET9_0_OR_GREATER
        where TArg0 : allows ref struct where TArg1 : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var enumerator = new FormatSegmentEnumerator(format);

        while (enumerator.MoveNext())
        {
            AppendSegment(enumerator.Current, format, provider, 2, arg0, arg1, default(nint));
        }
    }

    /// <summary>
    ///     Appends the string returned by processing a composite format string, which contains zero or more format items,
    ///     to this instance. Each format item is replaced by the string representation of any of the arguments using a
    ///     specified format provider.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <typeparam name="TArg1">The type of the second object to format.</typeparam>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="AStringCompositeFormat" />.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
    /// <exception cref="FormatException">
    ///     The index of a format item is greater than or equal to the number of supplied
    ///     arguments.
    /// </exception>
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TArg0, TArg1>(IFormatProvider? provider,
        AStringCompositeFormat format,
        TArg0 arg0,
        TArg1 arg1)
#if NET9_0_OR_GREATER
        where TArg0 : allows ref struct where TArg1 : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(format);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(format.MinimumArgumentCount, 3);

        foreach (var segment in format.Segments)
        {
            AppendSegment(segment, format.Format, provider, 2, arg0, arg1, default(nint));
        }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TArg0>(IFormatProvider? provider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0)
#if NET9_0_OR_GREATER
        where TArg0 : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var enumerator = new FormatSegmentEnumerator(format);

        while (enumerator.MoveNext())
        {
            AppendSegment(enumerator.Current, format, provider, 1, arg0, default(nint), default(nint));
        }
    }

    /// <summary>
    ///     Appends the string returned by processing a composite format string, which contains zero or more format items,
    ///     to this instance. Each format item is replaced by the string representation of any of the arguments using a
    ///     specified format provider.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="AStringCompositeFormat" />.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
    /// <exception cref="FormatException">
    ///     The index of a format item is greater than or equal to the number of supplied
    ///     arguments.
    /// </exception>
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TArg0>(IFormatProvider? provider, AStringCompositeFormat format, TArg0 arg0)

#if NET9_0_OR_GREATER
        where TArg0 : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(format);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(format.MinimumArgumentCount, 3);

        foreach (var segment in format.Segments)
        {
            AppendSegment(segment, format.Format, provider, 1, arg0, default(nint), default(nint));
        }
    }
    // internal void AppendFormatInternal<T>(IFormatProvider? provider,
    //     T arg,
    //     int width,
    //     [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format)
    // {
    //     ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
    //
    //     var cf = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
    //
    //     var cfs = cf?.Format(format.ToString(), arg, provider);
    //
    //     if (cfs == null) { AppendFormatInternal(arg, width, format, provider); }
    //     else { AppendFormatInternal(cfs,             width, format); }
    // }

    private void AppendSegment<T>(Segment segment,
        ReadOnlySpan<char> format,
        IFormatProvider? provider,
        ReadOnlySpan<T> args)
    {
        switch (segment.Type)
        {
            case SegmentType.Normal:
                Append(format[segment.Range]);
                break;
            case SegmentType.EscapedOpenBracket:
                Append('{');
                break;
            case SegmentType.EscapedCloseBracket:
                Append('}');
                break;
            case SegmentType.Argument:
                var index = segment.ArgumentIndex;
                if (index < 0 || index >= args.Length) { throw new FormatException(); } // TODO

                AppendFormatInternal(args[index], segment.Alignment, format[segment.Range], null, provider);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AppendSegment<TArg0, TArg1, TArg2>(Segment segment,
        ReadOnlySpan<char> format,
        IFormatProvider? provider,
        int maxArgument,
        TArg0 arg0,
        TArg1 arg1,
        TArg2 arg2)
#if NET9_0_OR_GREATER
        where TArg0 : allows ref struct where TArg1 : allows ref struct where TArg2 : allows ref struct
#endif
    {
        switch (segment.Type)
        {
            case SegmentType.Normal:
                Append(format[segment.Range]);
                break;
            case SegmentType.EscapedOpenBracket:
                Append('{');
                break;
            case SegmentType.EscapedCloseBracket:
                Append('}');
                break;
            case SegmentType.Argument:
                var index = segment.ArgumentIndex;
                if (index < 0 || index >= maxArgument) { throw new FormatException(); } // TODO

                switch (index)
                {
                    case 0:
                        AppendFormatInternal(arg0, segment.Alignment, format[segment.Range], null, provider);
                        break;
                    case 1:
                        AppendFormatInternal(arg1, segment.Alignment, format[segment.Range], null, provider);
                        break;
                    case 2:
                        AppendFormatInternal(arg2, segment.Alignment, format[segment.Range], null, provider);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal void AppendFormatInternal<T>(T value,
        int width,
        ReadOnlySpan<char> formatSpan,
        string? formatString,
        IFormatProvider? provider)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
        const int stackAllocThreshold = 1024;

        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        if (width <= 0) { AppendFormatLeft(ref this, value, width, formatSpan, formatString, provider); }
        else { AppendFormatRight(ref this, value, width, formatSpan, formatString, provider); }

        static void AppendFormatLeft(ref ValueStringAppender self,
            T value,
            int width,
            ReadOnlySpan<char> formatSpan,
            string? formatString,
            IFormatProvider? provider = null)
        {
            width *= -1;

            int?      charsWritten;
            const int maxRetry = 2;
            var       i        = 0;

            var guestLength = GetGuestLength<T>();
            while (true)
            {
                charsWritten = TryAppend(ref self, value, guestLength * (i + 1), formatSpan, provider);
                if (charsWritten.HasValue || i++ >= maxRetry) { break; }
            }

            if (!charsWritten.HasValue)
            {
                if (formatString == null && !formatSpan.IsEmpty) { formatString = formatSpan.ToString(); }

                var str = FormatterCache.Format(value, formatString, provider);
                charsWritten = str.Length;
                self.Append(str);
            }

            var padding = width - charsWritten.Value;
            if (width <= 0 || padding <= 0) { return; }

            self.Append(' ', padding);

            static int? TryAppend(ref ValueStringAppender self,
                T value,
                int guestLength,
                ReadOnlySpan<char> format,
                IFormatProvider? provider) =>
                guestLength > stackAllocThreshold
                    ? UseArrayBuffer(ref self, value, format, provider, guestLength)
                    : Fill(ref self, value, format, provider, stackalloc char[guestLength]);

            static int? UseArrayBuffer(ref ValueStringAppender self,
                T value,
                ReadOnlySpan<char> format,
                IFormatProvider? provider,
                int length)
            {
                var array = ArrayPool<char>.Shared.Rent(length);
                try { return Fill(ref self, value, format, provider, array.AsSpan(0, length)); }
                finally { ArrayPool<char>.Shared.Return(array); }
            }

            static int? Fill(ref ValueStringAppender self,
                T value,
                ReadOnlySpan<char> format,
                IFormatProvider? provider,
                Span<char> buffer)
            {
                if (!FormatterCache.TryFormat(value, buffer, out var charsWritten, format, provider)) { return null; }

                self.Append(buffer[..charsWritten]);
                return charsWritten;
            }
        }

        static void AppendFormatRight(ref ValueStringAppender self,
            T arg,
            int width,
            ReadOnlySpan<char> formatSpan,
            string? formatString,
            IFormatProvider? provider)
        {
            const int maxRetry = 2;
            var       i        = 0;

            var handler = default(Handler);
            try
            {
                var guestLength = GetGuestLength<T>();
                while (true)
                {
                    if (TryFill(arg, formatSpan, provider, guestLength, out handler) || i++ >= maxRetry) { break; }
                }

                if (handler.Span.Length == 0)
                {
                    if (formatString == null && !formatSpan.IsEmpty) { formatString = formatSpan.ToString(); }

                    var str = FormatterCache.Format(arg, formatString, provider);
                    handler = new Handler(str.AsSpan());
                }


                var padding = width - handler.Span.Length;
                if (padding > 0)
                {
                    self.Append(' ', padding); // TODO Fill Method is too slow.
                }

                self.Append(handler.Span);
            }
            finally { handler.Dispose(); }

            static bool TryFill(T arg,
                ReadOnlySpan<char> format,
                IFormatProvider? provider,
                int guestLength,
                out Handler handler)
            {
                var array = ArrayPool<char>.Shared.Rent(guestLength);

                if (!FormatterCache.TryFormat(arg, array.AsSpan(), out var charsWritten, format, provider))
                {
                    handler = default;
                    return false;
                }

                handler = new Handler(array, charsWritten);
                return true;
            }
        }
    }

    private ref struct Handler : IDisposable
    {
        private readonly char[]? _buffer;

        public ReadOnlySpan<char> Span { get; }

        public Handler(char[] buffer, int spanSize)
        {
            _buffer = buffer;
            Span    = _buffer.AsSpan(0, spanSize);
        }

        public Handler(ReadOnlySpan<char> buffer) => Span = buffer;

        public void Dispose()
        {
            if (_buffer == null) { return; }

            ArrayPool<char>.Shared.Return(_buffer);
        }
    }

#if NET8_0_OR_GREATER
    [PublicAPI]
    public delegate void SelectParameter<in TState>([InstantHandle] ref ParameterSender sender,
        int parameterIndex,
        TState state)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
    ;

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var array = ArrayPool<char>.Shared.Rent(format.Length);
        try
        {
            format.CopyTo(array);
            var enumerator = new FormatSegmentEnumerator(format);
            while (enumerator.MoveNext())
            {
                AppendSegment(enumerator.Current, array.AsMemory(0, format.Length), null, state, selector);
            }
        }
        finally { ArrayPool<char>.Shared.Return(array); }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
        =>
            AppendFormat(format.AsMemory(), state, selector);


    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlyMemory<char> format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var enumerator = new FormatSegmentEnumerator(format.Span);
        while (enumerator.MoveNext()) { AppendSegment(enumerator.Current, format, null, state, selector); }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>(AStringCompositeFormat format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(format);

        foreach (var segment in format.Segments)
        {
            AppendSegment(segment, format.Format.AsMemory(), null, state, selector);
        }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>(IFormatProvider? formatProvider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var array = ArrayPool<char>.Shared.Rent(format.Length);
        try
        {
            format.CopyTo(array);
            var enumerator = new FormatSegmentEnumerator(format);
            while (enumerator.MoveNext())
            {
                AppendSegment(enumerator.Current, array.AsMemory(0, format.Length), formatProvider, state, selector);
            }
        }
        finally { ArrayPool<char>.Shared.Return(array); }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>(IFormatProvider? formatProvider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
        =>
            AppendFormat(formatProvider, format.AsMemory(), state, selector);

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>(IFormatProvider? formatProvider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlyMemory<char> format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var enumerator = new FormatSegmentEnumerator(format.Span);
        while (enumerator.MoveNext()) { AppendSegment(enumerator.Current, format, formatProvider, state, selector); }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>(IFormatProvider? formatProvider,
        AStringCompositeFormat format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(format);

        foreach (var segment in format.Segments)
        {
            AppendSegment(segment, format.Format.AsMemory(), formatProvider, state, selector);
        }
    }

    private void AppendSegment<TState>(Segment segment,
        ReadOnlyMemory<char> format,
        IFormatProvider? provider,
        TState state,
        SelectParameter<TState> selector)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
    {
        switch (segment.Type)
        {
            case SegmentType.Normal:
                Append(format[segment.Range]);
                break;
            case SegmentType.EscapedOpenBracket:
                Append('{');
                break;
            case SegmentType.EscapedCloseBracket:
                Append('}');
                break;
            case SegmentType.Argument:
                var sender = new ParameterSender(ref this, provider, segment.Alignment, format[segment.Range]);

                try { selector.Invoke(ref sender, segment.ArgumentIndex, state); }
                catch (Exception ex)
                {
                    throw new FormatException($"Error formatting argument {segment.ArgumentIndex}", ex);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public ref struct ParameterSender
    {
        private ref      ValueStringAppender  _valueStringBuilder;
        private readonly IFormatProvider?     _formatProvider;
        private readonly int                  _width;
        private readonly ReadOnlyMemory<char> _formatMemory;

        internal ParameterSender(ref ValueStringAppender valueStringAppender,
            IFormatProvider? formatProvider,
            int width,
            ReadOnlyMemory<char> format)
        {
            _formatProvider     = formatProvider;
            _width              = width;
            _formatMemory       = format;
            _valueStringBuilder = ref valueStringAppender;
        }

        [PublicAPI]
        public void Send<T>(T? value)
        {
            _valueStringBuilder.AppendFormatInternal(value, _width, _formatMemory.Span, null, _formatProvider);
        }
    }
#endif
}