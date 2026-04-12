using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Astra.Text.Models;
using JetBrains.Annotations;
using Pure = System.Diagnostics.Contracts.PureAttribute;

namespace Astra.Text;

[PublicAPI]
public static class AString
{
    [Pure]
    [PublicAPI]
    public static ValueStringBuilder CreateBuilder() => new();

    [Pure]
    [PublicAPI]
    public static CharBufferWriter CreateBufferWriter() => new();

    [Pure]
    [PublicAPI]
    public static Utf8BufferWriter CreateUtf8BufferWriter() => new();

    [Pure]
    [PublicAPI]
    public static string Concat<T>(IEnumerable<T> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        return Join([], values);
    }

    [Pure]
    [PublicAPI]
    public static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1)
    {
        using var builder = new ValueStringBuilder();
        builder.Append(str0);
        builder.Append(str1);
        return builder.ToString();
    }

    [Pure]
    [PublicAPI]
    public static string Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2)
    {
        using var builder = new ValueStringBuilder();
        builder.Append(str0);
        builder.Append(str1);
        builder.Append(str2);
        return builder.ToString();
    }

    [Pure]
    [PublicAPI]
    public static string Concat(ReadOnlySpan<char> str0,
        ReadOnlySpan<char> str1,
        ReadOnlySpan<char> str2,
        ReadOnlySpan<char> str3)
    {
        using var builder = new ValueStringBuilder();
        builder.Append(str0);
        builder.Append(str1);
        builder.Append(str2);
        builder.Append(str3);
        return builder.ToString();
    }

    internal static string Concat(ReadOnlySpan<char> str0,
        ReadOnlySpan<char> str1,
        ReadOnlySpan<char> str2,
        ReadOnlySpan<char> str3,
        ReadOnlySpan<char> str4)
    {
        using var builder = new ValueStringBuilder();
        builder.Append(str0);
        builder.Append(str1);
        builder.Append(str2);
        builder.Append(str3);
        builder.Append(str4);
        return builder.ToString();
    }

    /// <summary>Concatenates the elements of a specified span of <see cref="string" />.</summary>
    /// <param name="values">A span of <see cref="string" /> instances.</param>
    /// <returns>The concatenated elements of <paramref name="values" />.</returns>
    [Pure]
    [PublicAPI]
    public static string Concat(params ReadOnlySpan<string?> values)
    {
        using var builder = new ValueStringBuilder();
        foreach (var value in values) { builder.Append(value); }

        return builder.ToString();
    }

    [Pure]
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public static string Format<TArg0>([StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0) =>
        Format(null, format, arg0);

    [Pure]
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public static string Format<TArg0>(IFormatProvider? provider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0)
    {
        using var builder = new ValueStringBuilder();
        builder.AppendFormat(provider, format, arg0);
        return builder.ToString();
    }

    [Pure]
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public static string Format<TArg0, TArg1>(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0,
        TArg1 arg1) =>
        Format(null, format, arg0, arg1);

    [Pure]
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public static string Format<TArg0, TArg1>(IFormatProvider? provider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0,
        TArg1 arg1)
    {
        using var builder = new ValueStringBuilder();
        builder.AppendFormat(provider, format, arg0, arg1);
        return builder.ToString();
    }

    [Pure]
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public static string Format<TArg0, TArg1, TArg2>(
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0,
        TArg1 arg1,
        TArg2 arg2) =>
        Format(null, format, arg0, arg1, arg2);

    [Pure]
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public static string Format<TArg0, TArg1, TArg2>(IFormatProvider? provider,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)] ReadOnlySpan<char> format,
        TArg0 arg0,
        TArg1 arg1,
        TArg2 arg2)
    {
        using var builder = new ValueStringBuilder();
        builder.AppendFormat(provider, format, arg0, arg1, arg2);
        return builder.ToString();
    }

    /// <summary>
    ///     Replaces the format item or items in a <see cref="AStringCompositeFormat" /> with the string representation of
    ///     the corresponding objects. A parameter supplies culture-specific formatting information.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <param name="format">A <see cref="AStringCompositeFormat" />.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <returns>The formatted string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
    /// <exception cref="FormatException">
    ///     The index of a format item is greater than or equal to the number of supplied
    ///     arguments.
    /// </exception>
    [Pure]
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public static string Format<TArg0>(AStringCompositeFormat format, TArg0 arg0) => Format<TArg0>(null, format, arg0);

    /// <summary>
    ///     Replaces the format item or items in a <see cref="AStringCompositeFormat" /> with the string representation of
    ///     the corresponding objects. A parameter supplies culture-specific formatting information.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="AStringCompositeFormat" />.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <returns>The formatted string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
    /// <exception cref="FormatException">
    ///     The index of a format item is greater than or equal to the number of supplied
    ///     arguments.
    /// </exception>
    [Pure]
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public static string Format<TArg0>(IFormatProvider? provider, AStringCompositeFormat format, TArg0 arg0)
    {
        ArgumentNullException.ThrowIfNull(format);

        using var builder = new ValueStringBuilder();
        builder.AppendFormat(provider, format, arg0);
        return builder.ToString();
    }

    /// <summary>
    ///     Replaces the format item or items in a <see cref="AStringCompositeFormat" /> with the string representation of
    ///     the corresponding objects. A parameter supplies culture-specific formatting information.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <typeparam name="TArg1">The type of the second object to format.</typeparam>
    /// <param name="format">A <see cref="AStringCompositeFormat" />.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <returns>The formatted string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
    /// <exception cref="FormatException">
    ///     The index of a format item is greater than or equal to the number of supplied
    ///     arguments.
    /// </exception>
    [Pure]
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public static string Format<TArg0, TArg1>(AStringCompositeFormat format, TArg0 arg0, TArg1 arg1) =>
        Format<TArg0, TArg1>(null, format, arg0, arg1);

    /// <summary>
    ///     Replaces the format item or items in a <see cref="AStringCompositeFormat" /> with the string representation of
    ///     the corresponding objects. A parameter supplies culture-specific formatting information.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <typeparam name="TArg1">The type of the second object to format.</typeparam>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="AStringCompositeFormat" />.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <returns>The formatted string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
    /// <exception cref="FormatException">
    ///     The index of a format item is greater than or equal to the number of supplied
    ///     arguments.
    /// </exception>
    [Pure]
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public static string Format<TArg0, TArg1>(IFormatProvider? provider,
        AStringCompositeFormat format,
        TArg0 arg0,
        TArg1 arg1)
    {
        ArgumentNullException.ThrowIfNull(format);

        using var builder = new ValueStringBuilder();
        builder.AppendFormat(provider, format, arg0, arg1);
        return builder.ToString();
    }

    /// <summary>
    ///     Replaces the format item or items in a <see cref="AStringCompositeFormat" /> with the string representation of
    ///     the corresponding objects. A parameter supplies culture-specific formatting information.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <typeparam name="TArg1">The type of the second object to format.</typeparam>
    /// <typeparam name="TArg2">The type of the third object to format.</typeparam>
    /// <param name="format">A <see cref="AStringCompositeFormat" />.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <param name="arg2">The third object to format.</param>
    /// <returns>The formatted string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
    /// <exception cref="FormatException">
    ///     The index of a format item is greater than or equal to the number of supplied
    ///     arguments.
    /// </exception>
    [Pure]
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public static string
        Format<TArg0, TArg1, TArg2>(AStringCompositeFormat format, TArg0 arg0, TArg1 arg1, TArg2 arg2) =>
        Format(null, format, arg0, arg1, arg2);

    /// <summary>
    ///     Replaces the format item or items in a <see cref="AStringCompositeFormat" /> with the string representation of
    ///     the corresponding objects. A parameter supplies culture-specific formatting information.
    /// </summary>
    /// <typeparam name="TArg0">The type of the first object to format.</typeparam>
    /// <typeparam name="TArg1">The type of the second object to format.</typeparam>
    /// <typeparam name="TArg2">The type of the third object to format.</typeparam>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="format">A <see cref="AStringCompositeFormat" />.</param>
    /// <param name="arg0">The first object to format.</param>
    /// <param name="arg1">The second object to format.</param>
    /// <param name="arg2">The third object to format.</param>
    /// <returns>The formatted string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="format" /> is null.</exception>
    /// <exception cref="FormatException">
    ///     The index of a format item is greater than or equal to the number of supplied
    ///     arguments.
    /// </exception>
    [Pure]
    [PublicAPI]
    [StringFormatMethod(nameof(format))]
    public static string Format<TArg0, TArg1, TArg2>(IFormatProvider? provider,
        AStringCompositeFormat format,
        TArg0 arg0,
        TArg1 arg1,
        TArg2 arg2)
    {
        ArgumentNullException.ThrowIfNull(format);

        using var builder = new ValueStringBuilder();
        builder.AppendFormat(provider, format, arg0, arg1, arg2);
        return builder.ToString();
    }

    [Pure]
    [PublicAPI]
    public static string Join<T>(char separator, ReadOnlySpan<T?> value, int startIndex, int count)
    {
        using var builder = new ValueStringBuilder();
        builder.AppendJoin(separator, value[startIndex..(startIndex + count)]);
        return builder.ToString();
    }

    [Pure]
    [PublicAPI]
    public static string Join<T>(ReadOnlySpan<char> separator, ReadOnlySpan<T?> value, int startIndex, int count)
    {
        using var builder = new ValueStringBuilder();
        builder.AppendJoin(separator, value[startIndex..(startIndex + count)]);
        return builder.ToString();
    }

    [Pure]
    [PublicAPI]
    public static string Join<T>(char separator, IEnumerable<T> values)
    {
        using var builder = new ValueStringBuilder();
        builder.AppendJoin(separator, values);
        return builder.ToString();
    }

    /// <summary>Concatenates a span of strings, using the specified separator between each member.</summary>
    /// <param name="separator">
    ///     The character to use as a separator. <paramref name="separator" /> is included in the returned
    ///     string only if <paramref name="values" /> has more than one element.
    /// </param>
    /// <param name="values">A span that contains the elements to concatenate.</param>
    /// <returns>
    ///     A string that consists of the elements of <paramref name="values" /> delimited by the
    ///     <paramref name="separator" /> string. -or- <see cref="String.Empty" /> if <paramref name="values" /> has zero
    ///     elements.
    /// </returns>
    [Pure]
    [PublicAPI]
    public static string Join<T>(char separator, ReadOnlySpan<T> values)
    {
        using var builder = new ValueStringBuilder();
        builder.AppendJoin(separator, values);
        return builder.ToString();
    }

    [Pure]
    [PublicAPI]
    public static string Join<T>(ReadOnlySpan<char> separator, IEnumerable<T> values)
    {
        using var builder = new ValueStringBuilder();
        builder.AppendJoin(separator, values);
        return builder.ToString();
    }

    /// <summary>Concatenates a span of strings, using the specified separator between each member.</summary>
    /// <param name="separator">
    ///     The string to use as a separator. <paramref name="separator" /> is included in the returned
    ///     string only if <paramref name="values" /> has more than one element.
    /// </param>
    /// <param name="values">A span that contains the elements to concatenate.</param>
    /// <returns>
    ///     A string that consists of the elements of <paramref name="values" /> delimited by the
    ///     <paramref name="separator" /> string. -or- <see cref="String.Empty" /> if <paramref name="values" /> has zero
    ///     elements.
    /// </returns>
    [Pure]
    [PublicAPI]
    public static string Join<T>(ReadOnlySpan<char> separator, ReadOnlySpan<T> values)
    {
        using var builder = new ValueStringBuilder();
        builder.AppendJoin(separator, values);
        return builder.ToString();
    }
}