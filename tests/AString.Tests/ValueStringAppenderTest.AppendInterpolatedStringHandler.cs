using System.Globalization;

namespace Astra.Text.Tests;

public class ValueStringAppenderTestAppendInterpolatedStringHandler
{
    [Test]
    public async Task AppendFormattedInt()
    {
        using var sb1   = new ValueStringAppender();
        using var sb2   = new ValueStringAppender();
        var       value = 42;
        sb1.Append($"value={value}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(6, 1, sb1);
        handler.AppendLiteral("value=");
        handler.AppendFormatted(value);
        sb2.Append(ref handler);

        await Assert.That(sb1.ToString()).IsEqualTo($"value={value}");
        await Assert.That(sb2.ToString()).IsEqualTo($"value={value}");
    }

    [Test]
    public async Task AppendFormattedDouble()
    {
        using var sb1   = new ValueStringAppender();
        using var sb2   = new ValueStringAppender();
        var       value = 3.14;
        sb1.Append($"pi={value}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(3, 1, sb1);
        handler.AppendLiteral("pi=");
        handler.AppendFormatted(value);
        sb2.Append(ref handler);

        await Assert.That(sb1.ToString()).IsEqualTo($"pi={value}");
        await Assert.That(sb2.ToString()).IsEqualTo($"pi={value}");
    }

    [Test]
    public async Task AppendFormattedString()
    {
        using var sb1  = new ValueStringAppender();
        using var sb2  = new ValueStringAppender();
        var       name = "world";
        sb1.Append($"hello {name}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(6, 1, sb1);
        handler.AppendLiteral("hello ");
        handler.AppendFormatted(name);
        sb2.Append(ref handler);

        await Assert.That(sb1.ToString()).IsEqualTo("hello world");
        await Assert.That(sb2.ToString()).IsEqualTo("hello world");
    }

    [Test]
    public async Task AppendFormattedWithFormat()
    {
        using var sb1   = new ValueStringAppender();
        using var sb2   = new ValueStringAppender();
        var       value = 255;
        sb1.Append($"{value:X}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(0, 1, sb1);
        handler.AppendFormatted(value, "X");
        sb2.Append(ref handler);

        await Assert.That(sb1.ToString()).IsEqualTo($"{value:X}");
        await Assert.That(sb2.ToString()).IsEqualTo($"{value:X}");
    }

    [Test]
    public async Task AppendFormattedWithRightAlignment()
    {
        using var sb1   = new ValueStringAppender();
        using var sb2   = new ValueStringAppender();
        var       value = 42;
        sb1.Append($"{value,10}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(0, 1, sb1);
        handler.AppendFormatted(value, 10);
        sb2.Append(ref handler);

        await Assert.That(sb1.ToString()).IsEqualTo($"{value,10}");
        await Assert.That(sb2.ToString()).IsEqualTo($"{value,10}");
    }

    [Test]
    public async Task AppendFormattedWithLeftAlignment()
    {
        using var sb1   = new ValueStringAppender();
        using var sb2   = new ValueStringAppender();
        var       value = 42;
        sb1.Append($"{value,-10}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(0, 1, sb1);
        handler.AppendFormatted(value, -10);
        sb2.Append(ref handler);

        await Assert.That(sb1.ToString()).IsEqualTo($"{value,-10}");
        await Assert.That(sb2.ToString()).IsEqualTo($"{value,-10}");
    }

    [Test]
    public async Task AppendFormattedAlignmentNopadding()
    {
        using var sb1   = new ValueStringAppender();
        using var sb2   = new ValueStringAppender();
        var       value = "toolongvalue";
        sb1.Append($"{value,3}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(0, 1, sb1);
        handler.AppendFormatted(value, 3);
        sb2.Append(ref handler);

        // alignment less than string length — no padding
        await Assert.That(sb1.ToString()).IsEqualTo($"{value,3}");
        await Assert.That(sb2.ToString()).IsEqualTo($"{value,3}");
    }

    [Test]
    public async Task AppendSpanRightAlignment()
    {
        using var          sb1  = new ValueStringAppender();
        using var          sb2  = new ValueStringAppender();
        ReadOnlySpan<char> span = "hi";
        sb1.Append($"{span,6}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(0, 1, sb1);
        handler.AppendFormatted(span, 6);
        sb2.Append(ref handler);

        await Assert.That(sb1.ToString()).IsEqualTo("    hi");
        await Assert.That(sb2.ToString()).IsEqualTo("    hi");
    }

    [Test]
    public async Task AppendSpanLeftAlignment()
    {
        using var          sb1  = new ValueStringAppender();
        using var          sb2  = new ValueStringAppender();
        ReadOnlySpan<char> span = "hi";
        sb1.Append($"{span,-6}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(0, 1, sb1);
        handler.AppendFormatted(span, -6);
        sb2.Append(ref handler);

        await Assert.That(sb1.ToString()).IsEqualTo("hi    ");
        await Assert.That(sb2.ToString()).IsEqualTo("hi    ");
    }

    [Test]
    public async Task AppendSpanNoAlignment()
    {
        using var          sb1  = new ValueStringAppender();
        using var          sb2  = new ValueStringAppender();
        ReadOnlySpan<char> span = "hello";
        sb1.Append($"{span}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(0, 1, sb1);
        handler.AppendFormatted(span);
        sb2.Append(ref handler);

        await Assert.That(sb1.ToString()).IsEqualTo("hello");
        await Assert.That(sb2.ToString()).IsEqualTo("hello");
    }

    [Test]
    public async Task AppendMultipleFormattedValues()
    {
        using var sb1 = new ValueStringAppender();
        using var sb2 = new ValueStringAppender();
        var       a   = 1;
        var       b   = "two";
        var       c   = 3.0;
        sb1.Append($"{a} {b} {c}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(2, 3, sb1);
        handler.AppendFormatted(a);
        handler.AppendLiteral(" ");
        handler.AppendFormatted(b);
        handler.AppendLiteral(" ");
        handler.AppendFormatted(c);
        sb2.Append(ref handler);

        await Assert.That(sb1.ToString()).IsEqualTo($"{a} {b} {c}");
        await Assert.That(sb2.ToString()).IsEqualTo($"{a} {b} {c}");
    }

    [Test]
    public async Task AppendWithProvider()
    {
        using var sb1   = new ValueStringAppender();
        using var sb2   = new ValueStringAppender();
        var       value = 1234.5;
        sb1.Append(CultureInfo.InvariantCulture, $"{value:F2}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(0, 1, sb1, CultureInfo.InvariantCulture);
        handler.AppendFormatted(value, "F2");
        sb2.Append(CultureInfo.InvariantCulture, ref handler);

        var expected = value.ToString("F2", CultureInfo.InvariantCulture);
        await Assert.That(sb1.ToString()).IsEqualTo(expected);
        await Assert.That(sb2.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task AppendWithProviderCultureSpecific()
    {
        using var sb1   = new ValueStringAppender();
        using var sb2   = new ValueStringAppender();
        var       value = 1234567;
        sb1.Append(CultureInfo.InvariantCulture, $"{value:N0}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(0, 1, sb1, CultureInfo.InvariantCulture);
        handler.AppendFormatted(value, "N0");
        sb2.Append(CultureInfo.InvariantCulture, ref handler);

        var expected = value.ToString("N0", CultureInfo.InvariantCulture);
        await Assert.That(sb1.ToString()).IsEqualTo(expected);
        await Assert.That(sb2.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task AppendChained()
    {
        using var sb1 = new ValueStringAppender();
        using var sb2 = new ValueStringAppender();
        sb1.Append("foo");
        sb1.Append("bar");

        var h1 = new ValueStringAppender.AppendInterpolatedStringHandler(3, 0, sb1);
        h1.AppendLiteral("foo");
        sb2.Append(ref h1);

        var h2 = new ValueStringAppender.AppendInterpolatedStringHandler(3, 0, sb1);
        h2.AppendLiteral("bar");
        sb2.Append(ref h2);

        await Assert.That(sb1.ToString()).IsEqualTo("foobar");
        await Assert.That(sb2.ToString()).IsEqualTo("foobar");
    }

    [Test]
    public async Task AppendNullableValue()
    {
        using var sb1   = new ValueStringAppender();
        using var sb2   = new ValueStringAppender();
        int?      value = null;
        sb1.Append($"{value}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(0, 1, sb1);
        handler.AppendFormatted(value);
        sb2.Append(ref handler);

        await Assert.That(sb1.ToString()).IsEqualTo($"{value}");
        await Assert.That(sb2.ToString()).IsEqualTo($"{value}");
    }

    [Test]
    public async Task AppendFormattedGuid()
    {
        using var sb1  = new ValueStringAppender();
        using var sb2  = new ValueStringAppender();
        var       guid = Guid.Parse("12345678-1234-1234-1234-123456789abc");
        sb1.Append($"{guid:N}");

        var handler = new ValueStringAppender.AppendInterpolatedStringHandler(0, 1, sb1);
        handler.AppendFormatted(guid, "N");
        sb2.Append(ref handler);

        await Assert.That(sb1.ToString()).IsEqualTo(guid.ToString("N"));
        await Assert.That(sb2.ToString()).IsEqualTo(guid.ToString("N"));
    }
}