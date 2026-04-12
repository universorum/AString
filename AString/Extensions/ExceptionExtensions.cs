using System.Runtime.CompilerServices;
#if !NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

// ReSharper disable once CheckNamespace
namespace System;

internal static class ExceptionExtensions
{
    extension(ArgumentException)
    {
        public static void ThrowIfTrue(bool flag, string paramName)
        {
            if (!flag) { return; }

            throw new ArgumentOutOfRangeException(paramName, "Can't format argument");
        }

        public static void ThrowIfEmpty(ReadOnlySpan<char> argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument.IsEmpty) { throw new ArgumentException("The value cannot be an empty string.", paramName); }
        }
    }

#if !NET8_0_OR_GREATER
    extension(ArgumentNullException)
    {
        public static void ThrowIfNull([NotNull] object? argument,
            [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null) { throw new ArgumentNullException(paramName); }
        }
    }

    extension(ObjectDisposedException)
    {
        public static void ThrowIf([DoesNotReturnIf(true)] bool condition, object? instance)
        {
            if (condition) { throw new ObjectDisposedException(instance?.GetType().FullName); }
        }

        public static void ThrowIf([DoesNotReturnIf(true)] bool condition, Type type)
        {
            if (condition) { throw new ObjectDisposedException(type.FullName); }
        }
    }

    extension(ArgumentOutOfRangeException)
    {
        public static void ThrowIfNegative(int value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName,
                    value,
                    $"{paramName} ('{value}') must be a non-negative value.");
            }
        }

        public static void ThrowIfGreaterThan(int value,
            int other,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value > other)
            {
                throw new ArgumentOutOfRangeException(paramName,
                    value,
                    $"{paramName} ('{value}') must be less than or equal to '{other}'.");
            }
        }

        public static void ThrowIfLessThan(int value,
            int other,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value < other)
            {
                throw new ArgumentOutOfRangeException(paramName,
                    value,
                    $"{paramName} ('{value}') must be greater than or equal to '{other}'.");
            }
        }

        public static void ThrowIfGreaterThanOrEqual(int value,
            int other,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value >= other)
            {
                throw new ArgumentOutOfRangeException(paramName,
                    value,
                    $"{paramName} ('{value}') must be less than '{other}'.");
            }
        }

        public static void ThrowIfNegativeOrZero(int value,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value <= 0) { throw new ArgumentOutOfRangeException(paramName); }
        }
    }
#endif
}