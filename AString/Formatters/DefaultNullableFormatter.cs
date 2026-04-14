namespace Astra.Text.Formatters;

internal sealed class DefaultNullableFormatter<TFormattable> : ValueFormatter<TFormattable?> where TFormattable : struct
{
    private static readonly ValueFormatter<TFormattable> InnerFormatter;

    static DefaultNullableFormatter() =>
        InnerFormatter = (ValueFormatter<TFormattable>)FormatterHelper.CreateDefaultFormatter(typeof(TFormattable));

    public static DefaultNullableFormatter<TFormattable> Instance { get; } = new();

    public override bool TryFormat(in TFormattable? value,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider = null)
    {
        if (value.HasValue)
        {
            return InnerFormatter.TryFormat(value.Value, destination, out charsWritten, format, formatProvider);
        }

        charsWritten = 0;
        return true;
    }

    public override string Format(in TFormattable? value, string? format, IFormatProvider? formatProvider = null) =>
        !value.HasValue ? string.Empty : InnerFormatter.Format(value.Value, format, formatProvider);
}