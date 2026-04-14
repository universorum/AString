namespace Astra.Text.Formatters;

public abstract class ValueFormatter<T>
#if NET9_0_OR_GREATER
    where T : allows ref struct
#endif
{
    public virtual bool TryGetStringLength(in T value, out int length)
    {
        length = 0;
        return false;
    }

    public abstract bool TryFormat(in T value,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider = null);

    public abstract string Format(in T value, string? format, IFormatProvider? formatProvider = null);
}