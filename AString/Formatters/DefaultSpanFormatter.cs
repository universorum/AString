namespace Astra.Text.Formatters;

#if NET8_0_OR_GREATER
internal sealed class DefaultSpanFormatter<TFormattable> : ValueFormatter<TFormattable>
    where TFormattable : ISpanFormattable
{
    public static DefaultSpanFormatter<TFormattable> Instance { get; } = new();

    public override bool TryFormat(in TFormattable value,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider = null) =>
        value.TryFormat(destination, out charsWritten, format, formatProvider);

    public override string Format(in TFormattable value, string? format, IFormatProvider? formatProvider = null) =>
        value.ToString(format, formatProvider);
}
#endif