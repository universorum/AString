namespace Astra.Text.Formatters;

internal sealed class DefaultFormattableFormatter<T> : ValueFormatter<T> where T : IFormattable
{
    public static DefaultFormattableFormatter<T> Instance { get; } = new();

    public override bool TryFormat(in T _1,
        Span<char> _2,
        out int charsWritten,
        ReadOnlySpan<char> _3,
        IFormatProvider? _4 = null)
    {
        charsWritten = 0;
        return false;
    }

    public override string Format(in T value, string? format, IFormatProvider? formatProvider = null) =>
        value.ToString(format, formatProvider);
}