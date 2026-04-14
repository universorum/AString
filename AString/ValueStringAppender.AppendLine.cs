using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringAppender
{
    [PublicAPI]
    public void AppendLine() => Append(Environment.NewLine);

    /// <summary>Appends a string to the end of this builder.</summary>
    /// <param name="value">The string to append.</param>
    [PublicAPI]
    public void AppendLine(string? value)
    {
        Append(value.AsSpan());
        Append(Environment.NewLine);
    }

    [PublicAPI]
    public void AppendLine(char[]? value)
    {
        Append((ReadOnlySpan<char>)value.AsSpan());
        Append(Environment.NewLine);
    }

    /// <summary>Appends part of a string to the end of this builder.</summary>
    /// <param name="value">The string to append.</param>
    /// <param name="startIndex">The index to start in <paramref name="value" />.</param>
    /// <param name="count">The number of characters to read in <paramref name="value" />.</param>
    [PublicAPI]
    public void AppendLine(string? value, int startIndex, int count)
    {
        Append(value.AsSpan(startIndex, count));
        Append(Environment.NewLine);
    }

    /// <summary>Appends a range of characters to the end of this builder.</summary>
    /// <param name="value">The characters to append.</param>
    /// <param name="startIndex">The index to start in <paramref name="value" />.</param>
    /// <param name="charCount">The number of characters to read in <paramref name="value" />.</param>
    [PublicAPI]
    public void AppendLine(char[]? value, int startIndex, int charCount)
    {
        Append((ReadOnlySpan<char>)value.AsSpan(startIndex, charCount));
        Append(Environment.NewLine);
    }

    [PublicAPI]
    public void AppendLine(char value)
    {
        Append([value]);
        Append(Environment.NewLine);
    }

    /// <summary>Appends a character 0 or more times to the end of this builder.</summary>
    /// <param name="value">The character to append.</param>
    /// <param name="repeatCount">The number of times to append <paramref name="value" />.</param>
    [PublicAPI]
    public void AppendLine(char value, int repeatCount)
    {
        Append(value, repeatCount);
        Append(Environment.NewLine);
    }

    [PublicAPI]
    public void AppendLine(Memory<char> value)
    {
        Append((ReadOnlySpan<char>)value.Span);
        Append(Environment.NewLine);
    }

    [PublicAPI]
    public void AppendLine(ReadOnlyMemory<char> value)
    {
        Append(value.Span);
        Append(Environment.NewLine);
    }

    [PublicAPI]
    public void AppendLine(Span<char> value)
    {
        Append((ReadOnlySpan<char>)value);
        Append(Environment.NewLine);
    }

    [PublicAPI]
    public void AppendLine(ReadOnlySpan<char> value)
    {
        Append(value);
        Append(Environment.NewLine);
    }

    [PublicAPI]
    public void AppendLine<T>(T value)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
        Append(value);
        Append(Environment.NewLine);
    }

    /// <summary>
    ///     Appends the specified interpolated string followed by the default line terminator to the end of the current
    ///     StringBuilder object.
    /// </summary>
    /// <param name="handler">The interpolated string to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    [PublicAPI]
    public void AppendLine([InterpolatedStringHandlerArgument("")] ref AppendInterpolatedStringHandler handler)
    {
        Append(ref handler);
        Append(Environment.NewLine);
    }

    // ReSharper disable once EntityNameCapturedOnly.Global
    /// <summary>
    ///     Appends the specified interpolated string followed by the default line terminator to the end of the current
    ///     StringBuilder object.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="handler">The interpolated string to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    [PublicAPI]
    public void AppendLine(IFormatProvider? provider,
        [InterpolatedStringHandlerArgument("", nameof(provider))] ref AppendInterpolatedStringHandler handler)
    {
        Append(provider, ref handler);
        Append(Environment.NewLine);
    }

#if NETCOREAPP3_0_OR_GREATER
    [PublicAPI]
    public void AppendLine(Rune value)
    {
        Append(value);
        Append(Environment.NewLine);
    }
#endif
}