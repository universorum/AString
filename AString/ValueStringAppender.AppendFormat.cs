using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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

        while (enumerator.MoveNext())
        {
            var segment = enumerator.Current;
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
                    if (segment.ArgumentIndex >= args.Length) { throw new FormatException(); } // TODO

                    AppendFormatInternal(provider,
                        args[segment.ArgumentIndex],
                        segment.Alignment,
                        format[segment.FormatRange]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
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

        foreach (var segment in format.Segments)
        {
            switch (segment.Type)
            {
                case SegmentType.Normal:
                    Append(format.Format[segment.Range]);
                    break;
                case SegmentType.EscapedOpenBracket:
                    Append('{');
                    break;
                case SegmentType.EscapedCloseBracket:
                    Append('}');
                    break;
                case SegmentType.Argument:
                    if (segment.ArgumentIndex >= args.Length) { throw new FormatException(); } // TODO

                    AppendFormatInternal(provider,
                        args[segment.ArgumentIndex],
                        segment.Alignment,
                        format.Format.AsSpan(segment.FormatRange));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

#if NET8_0_OR_GREATER
    [PublicAPI]
    public delegate void SelectParameter<in TState>([InstantHandle] ref ParameterSender sender,
        int parameterIndex,
        TState state);

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var array = ArrayPool<char>.Shared.Rent(format.Length);
        try
        {
            format.CopyTo(array);
            var enumerator = new FormatSegmentEnumerator(format);

            while (enumerator.MoveNext())
            {
                var segment = enumerator.Current;
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
                        var (offset, length) = segment.FormatRange.GetOffsetAndLength(format.Length);
                        var sender = new ParameterSender(ref this,
                            null,
                            segment.Alignment,
                            array.AsMemory(offset, length));
                        selector.Invoke(ref sender, segment.ArgumentIndex, state);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        finally { ArrayPool<char>.Shared.Return(array); }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector) =>
        AppendFormat(format.AsMemory(), state, selector);

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlyMemory<char> format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var enumerator = new FormatSegmentEnumerator(format.Span);

        while (enumerator.MoveNext())
        {
            var segment = enumerator.Current;
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
                    var (offset, length) = segment.FormatRange.GetOffsetAndLength(format.Length);
                    var sender = new ParameterSender(ref this, null, segment.Alignment, format.Slice(offset, length));
                    selector.Invoke(ref sender, segment.ArgumentIndex, state);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>(AStringCompositeFormat format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(format);

        foreach (var segment in format.Segments)
        {
            switch (segment.Type)
            {
                case SegmentType.Normal:
                    Append(format.Format[segment.Range]);
                    break;
                case SegmentType.EscapedOpenBracket:
                    Append('{');
                    break;
                case SegmentType.EscapedCloseBracket:
                    Append('}');
                    break;
                case SegmentType.Argument:
                    var sender = new ParameterSender(ref this,
                        null,
                        segment.Alignment,
                        format.Format.AsMemory(segment.FormatRange));

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
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>(IFormatProvider? formatProvider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var array = ArrayPool<char>.Shared.Rent(format.Length);
        try
        {
            format.CopyTo(array);
            var enumerator = new FormatSegmentEnumerator(format);

            while (enumerator.MoveNext())
            {
                var segment = enumerator.Current;
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
                        var (offset, length) = segment.FormatRange.GetOffsetAndLength(format.Length);
                        var sender = new ParameterSender(ref this,
                            formatProvider,
                            segment.Alignment,
                            array.AsMemory(offset, length));
                        selector.Invoke(ref sender, segment.ArgumentIndex, state);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        finally { ArrayPool<char>.Shared.Return(array); }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>(IFormatProvider? formatProvider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] string format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector) =>
        AppendFormat(formatProvider, format.AsMemory(), state, selector);

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>(IFormatProvider? formatProvider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlyMemory<char> format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var enumerator = new FormatSegmentEnumerator(format.Span);

        while (enumerator.MoveNext())
        {
            var segment = enumerator.Current;
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
                    var (offset, length) = segment.FormatRange.GetOffsetAndLength(format.Length);
                    var sender = new ParameterSender(ref this,
                        formatProvider,
                        segment.Alignment,
                        format.Slice(offset, length));
                    selector.Invoke(ref sender, segment.ArgumentIndex, state);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TState>(IFormatProvider? formatProvider,
        AStringCompositeFormat format,
        TState state,
        [RequireStaticDelegate] SelectParameter<TState> selector)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(format);

        foreach (var segment in format.Segments)
        {
            switch (segment.Type)
            {
                case SegmentType.Normal:
                    Append(format.Format[segment.Range]);
                    break;
                case SegmentType.EscapedOpenBracket:
                    Append('{');
                    break;
                case SegmentType.EscapedCloseBracket:
                    Append('}');
                    break;
                case SegmentType.Argument:
                    var sender = new ParameterSender(ref this,
                        formatProvider,
                        segment.Alignment,
                        format.Format.AsMemory(segment.FormatRange));

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
    }
#endif

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TArg0, TArg1, TArg2>(IFormatProvider? provider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0,
        TArg1 arg1,
        TArg2 arg2)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var enumerator = new FormatSegmentEnumerator(format);

        while (enumerator.MoveNext())
        {
            var segment = enumerator.Current;
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
                    switch (segment.ArgumentIndex)
                    {
                        case 0:
                            AppendFormatInternal(provider, arg0, segment.Alignment, format[segment.FormatRange]);
                            break;
                        case 1:
                            AppendFormatInternal(provider, arg1, segment.Alignment, format[segment.FormatRange]);
                            break;
                        case 2:
                            AppendFormatInternal(provider, arg2, segment.Alignment, format[segment.FormatRange]);
                            break;
                        default:
                            throw new FormatException(""); // TODO
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
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
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(format);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(format.MinimumArgumentCount, 3);

        foreach (var segment in format.Segments)
        {
            switch (segment.Type)
            {
                case SegmentType.Normal:
                    Append(format.Format[segment.Range]);
                    break;
                case SegmentType.EscapedOpenBracket:
                    Append('{');
                    break;
                case SegmentType.EscapedCloseBracket:
                    Append('}');
                    break;
                case SegmentType.Argument:
                    switch (segment.ArgumentIndex)
                    {
                        case 0:
                            AppendFormatInternal(provider,
                                arg0,
                                segment.Alignment,
                                format.Format.AsSpan(segment.FormatRange));
                            break;
                        case 1:
                            AppendFormatInternal(provider,
                                arg1,
                                segment.Alignment,
                                format.Format.AsSpan(segment.FormatRange));
                            break;
                        case 2:
                            AppendFormatInternal(provider,
                                arg2,
                                segment.Alignment,
                                format.Format.AsSpan(segment.FormatRange));
                            break;
                        default:
                            throw new FormatException(""); // TODO
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TArg0, TArg1>(IFormatProvider? provider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0,
        TArg1 arg1)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var enumerator = new FormatSegmentEnumerator(format);

        while (enumerator.MoveNext())
        {
            var segment = enumerator.Current;
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
                    switch (segment.ArgumentIndex)
                    {
                        case 0:
                            AppendFormatInternal(provider, arg0, segment.Alignment, format[segment.FormatRange]);
                            break;
                        case 1:
                            AppendFormatInternal(provider, arg1, segment.Alignment, format[segment.FormatRange]);
                            break;
                        default:
                            throw new FormatException(""); // TODO
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(format);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(format.MinimumArgumentCount, 3);

        foreach (var segment in format.Segments)
        {
            switch (segment.Type)
            {
                case SegmentType.Normal:
                    Append(format.Format[segment.Range]);
                    break;
                case SegmentType.EscapedOpenBracket:
                    Append('{');
                    break;
                case SegmentType.EscapedCloseBracket:
                    Append('}');
                    break;
                case SegmentType.Argument:
                    switch (segment.ArgumentIndex)
                    {
                        case 0:
                            AppendFormatInternal(provider,
                                arg0,
                                segment.Alignment,
                                format.Format.AsSpan(segment.FormatRange));
                            break;
                        case 1:
                            AppendFormatInternal(provider,
                                arg1,
                                segment.Alignment,
                                format.Format.AsSpan(segment.FormatRange));
                            break;
                        default:
                            throw new FormatException(""); // TODO
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public void AppendFormat<TArg0>(IFormatProvider? provider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var enumerator = new FormatSegmentEnumerator(format);

        while (enumerator.MoveNext())
        {
            var segment = enumerator.Current;
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
                    switch (segment.ArgumentIndex)
                    {
                        case 0:
                            AppendFormatInternal(provider, arg0, segment.Alignment, format[segment.FormatRange]);
                            break;
                        default:
                            throw new FormatException(""); // TODO
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));
        ArgumentNullException.ThrowIfNull(format);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(format.MinimumArgumentCount, 3);

        foreach (var segment in format.Segments)
        {
            switch (segment.Type)
            {
                case SegmentType.Normal:
                    Append(format.Format[segment.Range]);
                    break;
                case SegmentType.EscapedOpenBracket:
                    Append('{');
                    break;
                case SegmentType.EscapedCloseBracket:
                    Append('}');
                    break;
                case SegmentType.Argument:
                    switch (segment.ArgumentIndex)
                    {
                        case 0:
                            AppendFormatInternal(provider,
                                arg0,
                                segment.Alignment,
                                format.Format.AsSpan(segment.FormatRange));
                            break;
                        default:
                            throw new FormatException(""); // TODO
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal void AppendFormatInternal<T>(IFormatProvider? provider,
        T arg,
        int width,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        var cf = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));

        var cfs = cf?.Format(format.ToString(), arg, provider);

        if (cfs == null) { AppendFormatInternal(arg, width, format, provider); }
        else { AppendFormatInternal(cfs,             width, format); }
    }

    private void AppendFormatInternal<T>(T arg,
        int width,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        IFormatProvider? provider = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(ValueStringAppender));

        if (width <= 0) { AppendFormatLeft(arg, width, format, provider); }
        else { AppendFormatRight(arg, width, format, provider); }
    }

    private void AppendFormatLeft<T>(T arg,
        int width,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        IFormatProvider? provider = null)
    {
        width *= -1;

        int?      charsWritten;
        const int maxRetry = 2;
        var       i        = 0;

        var guestLength = GetGuestLength<T>();
        while (true)
        {
            charsWritten = TryAppend(ref this, arg, guestLength * (i + 1), format, provider);
            if (charsWritten.HasValue || i++ >= maxRetry) { break; }
        }

        if (!charsWritten.HasValue)
        {
            var str = FormatterCache.Format(arg, format, provider);
            charsWritten = str.Length;
            Append(str);
        }

        var padding = width - charsWritten.Value;
        if (width <= 0 || padding <= 0) { return; }

        Append(' ', padding);

        static int? TryAppend(ref ValueStringAppender self,
            T value,
            int guestLength,
            ReadOnlySpan<char> format,
            IFormatProvider? provider)
        {
            Span<char> buffer = stackalloc char[guestLength];
            if (!FormatterCache.TryFormat(value, buffer, out var charsWritten, format, provider)) { return null; }

            self.Append(buffer[..charsWritten]);
            return charsWritten;
        }
    }

    private void AppendFormatRight<T>(T arg,
        int width,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        IFormatProvider? provider = null)
    {
        if (typeof(T) == typeof(string))
        {
            AppendFormatRightString(Unsafe.As<string>(arg), width);
            return;
        }

        var guestLength = GetGuestLength<T>();

        scoped ReadOnlySpan<char> ros;
        Span<char>                span = stackalloc char[guestLength];

        if (FormatterCache.TryFormat(arg, span, out var charsWritten, format, provider)) { ros = span[..charsWritten]; }
        else
        {
            span = stackalloc char[span.Length * 2];
            if (FormatterCache.TryFormat(arg, span, out charsWritten, format, provider)) { ros = span[..charsWritten]; }
            else
            {
                var str = FormatterCache.Format(arg, format, provider);
                ros          = str.AsSpan();
                charsWritten = str.Length;
            }
        }

        var padding = width - charsWritten;
        if (padding > 0)
        {
            Append(' ', padding); // TODO Fill Method is too slow.
        }

        Append(ros);
    }

    private void AppendFormatRightString(string? str, int width)
    {
        var padding = width - str?.Length ?? 0;
        if (padding > 0) { Append(' ', padding); }

        Append(str);
    }

#if NET8_0_OR_GREATER
    public ref struct ParameterSender
    {
        private ref      ValueStringAppender  _valueStringBuilder;
        private readonly IFormatProvider?     _formatProvider;
        private readonly int                  _width;
        private readonly ReadOnlyMemory<char> _format;

        internal ParameterSender(ref ValueStringAppender valueStringAppender,
            IFormatProvider? formatProvider,
            int width,
            ReadOnlyMemory<char> format)
        {
            _formatProvider = formatProvider;
            _width = width;
            _format = format;
            _valueStringBuilder = ref valueStringAppender;
        }

        [PublicAPI]
        public void Send<T>(T? value)
        {
            _valueStringBuilder.AppendFormatInternal(_formatProvider, value, _width, _format.Span);
        }
    }
#endif
}