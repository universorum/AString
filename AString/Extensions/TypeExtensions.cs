// ReSharper disable once CheckNamespace

namespace System;

internal static class TypeExtensions
{
#if !NET8_0_OR_GREATER
    public static bool IsAssignableTo(this Type type, Type otherType)
    {
        if (type == otherType) { return true; }

        if (otherType.IsInterface) { return type.GetInterfaces().Any(x => x == otherType); }

        var nullableType = type;
        while (nullableType != null)
        {
            if (nullableType == otherType) { return true; }

            nullableType = nullableType.BaseType;
        }

        return false;
    }
#endif
}