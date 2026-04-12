namespace Astra.Text.Formatters;

internal sealed class DefaultSpanFormatter<TFormattable> : ValueFormatter<TFormattable>
#if NET8_0_OR_GREATER
    where TFormattable : ISpanFormattable
{
    public static DefaultSpanFormatter<TFormattable> Instance { get; } = new();

    public override bool TryFormat(TFormattable value,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider = null) =>
        value.TryFormat(destination, out charsWritten, format, formatProvider);

    public override string Format(TFormattable value,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider = null) =>
        value.ToString(format.ToString(), formatProvider);
}
#else
    where TFormattable : IFormattable
{
    public override bool TryFormat(TFormattable value,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider = null)
    {
        var str = Format(value, format, formatProvider);
        if (str.Length > destination.Length)
        {
            charsWritten = 0;
            return false;
        }

        str.AsSpan().CopyTo(destination);
        charsWritten = str.Length;
        return true;
    }

    public override string
        Format(TFormattable value, ReadOnlySpan<char> format, IFormatProvider? formatProvider = null) =>
        value.ToString(format.ToString(), formatProvider);
}
#endif