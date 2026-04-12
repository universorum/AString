namespace Astra.Text.Formatters;

public abstract class ValueFormatter<T>
{
    public abstract bool TryFormat(T value,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider = null);

    public abstract string Format(T value, ReadOnlySpan<char> format, IFormatProvider? formatProvider = null);
}