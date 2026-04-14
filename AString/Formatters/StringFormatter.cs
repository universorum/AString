namespace Astra.Text.Formatters;

internal sealed class DefaultStringFormatter : ValueFormatter<string>
{
    public static DefaultStringFormatter Instance { get; } = new();

    public override bool TryGetStringLength(in string? value, out int length)
    {
        length = value?.Length ?? 0;
        return true;
    }

    public override bool TryFormat(in string? value,
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

    public override string Format(in string? value, string? _1, IFormatProvider? _2 = null) => value ?? string.Empty;
}