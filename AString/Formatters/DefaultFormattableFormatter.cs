namespace Astra.Text.Formatters;

internal sealed class DefaultFormattableFormatter<T> : ValueFormatter<T> where T : IFormattable
{
    public static DefaultFormattableFormatter<T> Instance { get;  } = new();

    public override bool TryFormat(T value,
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

        charsWritten = str.Length;
        str.AsSpan().CopyTo(destination);
        return true;
    }

    public override string Format(T value, ReadOnlySpan<char> format, IFormatProvider? formatProvider = null) =>
        value.ToString(format.ToString(), formatProvider);
}