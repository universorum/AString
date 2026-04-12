namespace Astra.Text.Formatters;

internal sealed class DefaultFormatter<T> : ValueFormatter<T>
{
    public static DefaultFormatter<T> Instance { get; } = new();

    public override bool TryFormat(T value,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider = null)
    {
#if NET8_0_OR_GREATER
        if (value is ISpanFormattable spanFormattable)
        {
            return spanFormattable.TryFormat(destination, out charsWritten, format, formatProvider);
        }
#endif

        var str = value is IFormattable formattable
            ? formattable.ToString(format.ToString(), formatProvider)
            : Format(value, format, formatProvider);

        if (str.Length > destination.Length)
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = str.Length;
        str.AsSpan().CopyTo(destination);
        return true;
    }

    public override string Format(T value, ReadOnlySpan<char> _1, IFormatProvider? _2 = null) =>
        value?.ToString() ?? string.Empty;
}