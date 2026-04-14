using Astra.Text.Formatters;
using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringAppender
{
    [PublicAPI]
    public static void RegisterTryFormat<T>(ValueFormatter<T> formatMethod)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
        FormatterCache<T>.Formatter = formatMethod;
    }

    private static class FormatterCache
    {
        public static bool TryGetStringLength<T>(in T value, out int length)
#if NET9_0_OR_GREATER
            where T : allows ref struct
#endif
            =>
                FormatterCache<T>.Formatter.TryGetStringLength(in value, out length);

        public static bool TryFormat<T>(in T value,
            Span<char> destination,
            out int written,
            ReadOnlySpan<char> format = default,
            IFormatProvider? formatProvider = null)
#if NET9_0_OR_GREATER
            where T : allows ref struct
#endif
            =>
                FormatterCache<T>.Formatter.TryFormat(value, destination, out written, format, formatProvider);

        public static string Format<T>(in T value, string? format = null, IFormatProvider? formatProvider = null)
#if NET9_0_OR_GREATER
            where T : allows ref struct
#endif
            =>
                FormatterCache<T>.Formatter.Format(value, format, formatProvider);
    }

    private static class FormatterCache<T>
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
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