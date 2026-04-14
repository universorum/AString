using System.Collections.Frozen;
using System.Reflection;

namespace Astra.Text.Formatters;

file static class DefaultEnumFormatter
{
    public static FrozenSet<char> SupportSet { get; } = new HashSet<char>
    {
        '\0',
        'G',
        'X',
        'D',
        'F'
    }.ToFrozenSet();
}

internal sealed class DefaultEnumFormatter<T> : ValueFormatter<T> where T : Enum
{
    private static FrozenDictionary<(T, char), string> _names;

    // ReSharper disable once StaticMemberInGenericType
    private static readonly bool IsFlag;

    static DefaultEnumFormatter()
    {
        IsFlag = typeof(T).GetCustomAttribute(typeof(FlagsAttribute)) != null;

        var enums = Enum.GetValues(typeof(T));
        var names = new Dictionary<(T, char), string>(enums.Length);

        for (var i = 0; i < enums.Length; i++)
        {
            var value       = (T)enums.GetValue(i)!;
            var formattable = (IFormattable)value;

            var empty = formattable.ToString(string.Empty, null);
            names.TryAdd((value, '\0'), empty);

            var g = formattable.ToString("G", null);
            names.TryAdd((value, 'G'), g);

            var x = formattable.ToString("X", null);
            names.TryAdd((value, 'X'), x);

            var d = formattable.ToString("D", null);
            names.TryAdd((value, 'D'), d);

            var f = formattable.ToString("F", null);
            names.TryAdd((value, 'F'), f);
        }

        _names = names.ToFrozenDictionary();
    }

    public static DefaultEnumFormatter<T> Instance { get; } = new();

    public override bool TryFormat(in T value,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? formatProvider = null)
    {
        if (formatProvider != null) { throw new NotImplementedException(); }

        if (format.Length > 1) { throw new ArgumentException("Format_InvalidEnumFormatSpecification"); } // TODO

        var formatChar = format.Length == 0 ? '\0' : char.ToUpperInvariant(format[0]);
        if (!DefaultEnumFormatter.SupportSet.Contains(formatChar))
        {
            throw new ArgumentException("Format_InvalidEnumFormatSpecification");
        } // TODO

        if (!_names.TryGetValue((value, formatChar), out var str))
        {
            if (!IsFlag) { throw new FormatException("Format_InvalidEnumFormatSpecification"); } // TODO

            str = value.ToString(format.ToString());

            var dict = new Dictionary<(T, char), string>();
            foreach (var (k, v) in _names) { dict.Add(k, v); }

            dict[(value, formatChar)] = str;

            var newDict = dict.ToFrozenDictionary();
            Interlocked.Exchange(ref _names, newDict);
        }

        var result = str.AsSpan().TryCopyTo(destination);
        charsWritten = result ? str.Length : 0;

        return result;
    }

    public override string Format(in T value, string? format, IFormatProvider? formatProvider = null)
    {
        if (formatProvider != null) { throw new NotImplementedException(); }

        format ??= string.Empty;

        if (format.Length > 1) { throw new ArgumentException("Format_InvalidEnumFormatSpecification"); } // TODO

        var formatChar = format.Length == 0 ? '\0' : char.ToUpperInvariant(format[0]);
        if (!DefaultEnumFormatter.SupportSet.Contains(formatChar))
        {
            throw new ArgumentException("Format_InvalidEnumFormatSpecification");
        } // TODO

        if (_names.TryGetValue((value, formatChar), out var str)) { return str; }

        if (!IsFlag) { throw new FormatException("Format_InvalidEnumFormatSpecification"); } // TODO

        str = value.ToString(format);

        var dict = new Dictionary<(T, char), string>();
        foreach (var (k, v) in _names) { dict.Add(k, v); }

        dict[(value, formatChar)] = str;

        var newDict = dict.ToFrozenDictionary();
        Interlocked.Exchange(ref _names, newDict);

        return str;
    }
}