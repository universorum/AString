namespace Astra.Text.Formatters;

internal static class FormatterHelper
{
    public static object CreateDefaultFormatter(Type type)
    {
        if (type == typeof(string)) { return DefaultStringFormatter.Instance; }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var innerType = type.GenericTypeArguments[0];

            return typeof(DefaultNullableFormatter<>).MakeGenericType(innerType)
                .GetProperty(nameof(DefaultNullableFormatter<>.Instance))!.GetValue(null)!;
        }

        if (type.IsEnum)
        {
            return typeof(DefaultEnumFormatter<>).MakeGenericType(type)
                .GetProperty(nameof(DefaultEnumFormatter<>.Instance))!.GetValue(null)!;
        }

#if NET8_0_OR_GREATER
        if (type.IsAssignableTo(typeof(ISpanFormattable)))
        {
            return typeof(DefaultSpanFormatter<>).MakeGenericType(type)
                .GetProperty(nameof(DefaultSpanFormatter<>.Instance))!.GetValue(null)!;
        }
#endif

        if (type.IsAssignableTo(typeof(IFormattable)))
        {
            return typeof(DefaultFormattableFormatter<>).MakeGenericType(type)
                .GetProperty(nameof(DefaultFormattableFormatter<>.Instance))!.GetValue(null)!;
        }

        return typeof(DefaultFormatter<>).MakeGenericType(type).GetProperty(nameof(DefaultFormatter<>.Instance))!
            .GetValue(null)!;
    }
}