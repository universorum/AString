namespace Astra.Text.Formatters;

internal sealed class DefaultStringFormatter : ValueFormatter<string>
{
    public static DefaultStringFormatter Instance { get; } = new();

    public override bool TryFormat(string? value,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? _ = null)
    {
        if (string.IsNullOrEmpty(value))
        {
            charsWritten = 0;
            return true;
        }

        if (destination.Length < value!.Length)
        {
            charsWritten = 0;
            return false;
        }

        value.AsSpan().CopyTo(destination);

        charsWritten = value.Length;
        return true;
    }

    public override string Format(string? value, ReadOnlySpan<char> format, IFormatProvider? _ = null) =>
        value ?? string.Empty;
}