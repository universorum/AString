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

        charsWritten = 0;
        return false;
    }

    public override string Format(T value, string? format, IFormatProvider? formatProvider = null) =>
        value is IFormattable formattable
            ? formattable.ToString(format, formatProvider)
            : value?.ToString() ?? string.Empty;
}