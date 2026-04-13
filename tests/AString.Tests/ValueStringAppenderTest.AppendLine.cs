using System.Globalization;
using System.Text;

namespace Astra.Text.Tests;

public class ValueStringAppenderTestAppendLine
{
    [Test]
    public async Task AppendLineEmpty()
    {
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        sb.AppendLine();
        bcl.AppendLine();
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    [Arguments("hello")]
    [Arguments("")]
    [Arguments("test line with spaces")]
    public async Task AppendLineString(string value)
    {
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        sb.AppendLine(value);
        bcl.AppendLine(value);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendLineNullString()
    {
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        sb.AppendLine((string?)null);
        bcl.AppendLine();
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendLineMultipleLines()
    {
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        sb.AppendLine("line1");
        sb.AppendLine("line2");
        sb.AppendLine("line3");
        bcl.AppendLine("line1");
        bcl.AppendLine("line2");
        bcl.AppendLine("line3");
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendLineAfterAppend()
    {
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        sb.Append("prefix");
        sb.AppendLine(" suffix");
        bcl.Append("prefix");
        bcl.AppendLine(" suffix");
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendLineInterpolated()
    {
        using var sb    = new ValueStringAppender();
        var       value = 42;
        sb.AppendLine($"value={value}");
        await Assert.That(sb.ToString()).IsEqualTo($"value={value}{Environment.NewLine}");
    }

    [Test]
    public async Task AppendLineInterpolatedWithProvider()
    {
        using var sb    = new ValueStringAppender();
        var       value = 3.14;
        sb.AppendLine(CultureInfo.InvariantCulture, $"pi={value:F2}");
        await Assert.That(sb.ToString()).IsEqualTo($"pi=3.14{Environment.NewLine}");
    }
}