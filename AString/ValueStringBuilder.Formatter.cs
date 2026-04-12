using Astra.Text.Formatters;
using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringBuilder
{
    [PublicAPI]
    public static void RegisterTryFormat<T>(ValueFormatter<T> formatMethod)
    {
        FormatterCache<T>.Formatter = formatMethod;
    }

    private static class FormatterCache
    {
        public static bool TryFormat<T>(T value,
            Span<char> destination,
            out int written,
            ReadOnlySpan<char> format = default,
            IFormatProvider? formatProvider = null) =>
            FormatterCache<T>.Formatter.TryFormat(value, destination, out written, format, formatProvider);

        public static string Format<T>(T value,
            ReadOnlySpan<char> format = default,
            IFormatProvider? formatProvider = null) =>
            FormatterCache<T>.Formatter.Format(value, format, formatProvider);
    }

    private static class FormatterCache<T>
    {
        private static ValueFormatter<T>? _defaultFormatter;

        public static ValueFormatter<T> DefaultFormatter =>
            _defaultFormatter ??= (ValueFormatter<T>)FormatterHelper.CreateDefaultFormatter(typeof(T));

        public static ValueFormatter<T> Formatter
        {
            get => field ??= DefaultFormatter;
            set;
        }
    }
}