using System.Numerics;

namespace Astra.Text.Tests;

public class ValueStringAppenderTestFormat
{
    [Test]
    [Arguments("{1}")]
    [Arguments("{-0}")]
    [Arguments("{-1}")]
    [Arguments("}")]
    [Arguments("{")]
    [Arguments("{}")]
    [Arguments("{A}")]
    [Arguments("{1A}")]
    [Arguments("{0x0}")]
    [Arguments("{\uff11}")] // Full-Width One
    [Arguments("{ }")]
    [Arguments("{ 1}")]
    [Arguments("{0 0}")]
    [Arguments("{0+0}")]
    [Arguments("{0")]
    [Arguments("{foo")]
    [Arguments("{{0}")]
    [Arguments("{{{0")]
    [Arguments("0}")]
    [Arguments("bar}")]
    [Arguments("{0}}")]
    [Arguments("0}}}")]
    [Arguments("{:0}")]
    [Arguments("{0{}")]
    [Arguments("{0{1}}")]
    [Arguments("{,0}")]
    [Arguments("{ 0,0}")]
    [Arguments("{,-0}")]
    [Arguments("{0,-}")]
    [Arguments("{0,- 0}")]
    [Arguments("{0,--0}")]
    [Arguments("{:}")]
    [Arguments("{,:}")]
    [Arguments(" { , : } ")]
    [Arguments("{:,}")]
    [Arguments("{::}")]
    [Arguments("{,,}")]
    [Arguments(@"{\0}")]
    [Arguments(@"{0\,0}")]
    [Arguments(@"{0,0\:}")]
    [Arguments(@"{0:\}}")]
    public void IncorrectFormat(string format)
    {
        Assert.Throws<FormatException>(() =>
        {
            using var builder = new ValueStringAppender();
            builder.AppendFormat(format, 9999);
        });
    }

    [Test]
    [Arguments("{1,0}{0,0}",  "right", "left")]
    [Arguments("{0,3}{1,-3}", "Foo",   "Foo")]
    [Arguments("{0,4}{1,-4}", "Foo",   "Foo")]
    public async Task String(string format, string arg0, string arg1)
    {
        using var sb = new ValueStringAppender();
        sb.AppendFormat(format, arg0, arg1);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
    }

    [Test]
    [Arguments("",                          100, 200)]
    [Arguments("abcdefg",                   100, 200)]
    [Arguments("{0}",                       100, 200)]
    [Arguments("{0}{1}",                    100, 200)]
    [Arguments("abc{0}def{1}",              100, 200)]
    [Arguments("abc{0}def{1}ghi",           100, 200)]
    [Arguments("",                          100, 200)]
    [Arguments("{0:}{1: }",                 100, 200)]
    [Arguments("}}{{{0}{{}}{1}{{",          123, 456)]
    [Arguments("{0:00000000}-{1:00000000}", 100, 200)]
    [Arguments("{0,1}{1,-1}",               1,   1)]
    [Arguments("{0,10}{1,-10}",             1,   1)]
    public async Task Int(string format, int arg0, int arg1)
    {
        using var sb = new ValueStringAppender();
        sb.AppendFormat(format, arg0, arg1);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
    }

    [Test]
    [Arguments("{1,-1}{0,1}", long.MinValue, long.MinValue)]
    public async Task Long(string format, long arg0, long arg1)
    {
        using var sb = new ValueStringAppender();
        sb.AppendFormat(format, arg0, arg1);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
    }

    [Test]
    [Arguments("abc{0}def{1}ghi",     100, 1)]
    [Arguments("abc{0:X}def{1:X}ghi", 100, 1)]
    public async Task IntNullable(string format, int? arg0, int? arg1)
    {
        using var sb = new ValueStringAppender();
        sb.AppendFormat(format, arg0, arg1);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
    }

    [Test]
    public async Task GuidNullable()
    {
        var format = "abc{0}def{1}ghi";
        var arg0   = (Guid?)Guid.Parse("0B3A57D0-5F85-463A-81EE-11C1E6C78E26");
        var arg1   = (Guid?)null;

        using var sb = new ValueStringAppender();
        sb.AppendFormat(format, arg0, arg1);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
    }

    [Test]
    public async Task DoubleNullable()
    {
        var format = "abc{0:e}def{1:e}ghi";
        var arg0   = (double?)Math.PI;
        var arg1   = (double?)null;

        using var sb = new ValueStringAppender();
        sb.AppendFormat(format, arg0, arg1);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
    }

    [Test]
    public async Task NInt()
    {
        var format = "abc{0:X}def{1:X}";
        var arg0   = nint.MaxValue;
        var arg1   = nint.MaxValue;

        using var sb = new ValueStringAppender();
        sb.AppendFormat(format, arg0, arg1);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
    }

    [Test]
    public async Task NuInt()
    {
        var format = "abc{0:X}def{1:X}";
        var arg0   = nuint.MaxValue;
        var arg1   = nuint.MaxValue;

        using var sb = new ValueStringAppender();
        sb.AppendFormat(format, arg0, arg1);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
    }

    public static IEnumerable<(string, object, object)> FormattableObjectTestData()
    {
        yield return ("abc{0:}def{1:F3}", default(Vector2), new Vector2(MathF.PI));
        yield return ("abc{0:E0}def{1:N}", new Vector3(float.MinValue, float.NaN, float.MaxValue),
            new Vector3(MathF.PI));
    }

    [Test]
    [MethodDataSource(nameof(FormattableObjectTestData))]
    public async Task FormattableObject(string format, object arg0, object arg1)
    {
        using var sb = new ValueStringAppender();
        sb.AppendFormat(format, arg0, arg1);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
    }

    [Test]
    public async Task Escape()
    {
        var format = @"{0:h\,h\:mm\:ss}";
        var span   = new TimeSpan(12, 34, 56);

        using var sb = new ValueStringAppender();
        sb.AppendFormat(format, span);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, span));
    }

    [Test]
    public async Task Unicode()
    {
        var format = "\u30cf\u30fc\u30c8: {0}, \u5bb6\u65cf: {1}(\u7d75\u6587\u5b57)";
        var arg0   = "\u2764";
        var arg1   = "\uD83D\uDC69\u200D\uD83D\uDC69\u200D\uD83D\uDC67\u200D\uD83D\uDC67";

        using var sb = new ValueStringAppender();
        sb.AppendFormat(format, arg0, arg1);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
    }

    [Test]
    public async Task AlignmentComponentStringLong()
    {
        var       format = "{1,-" + 1000 + "}{0," + 1000 + "}";
        var       arg0   = string.Empty;
        var       arg1   = long.MaxValue;
        using var sb     = new ValueStringAppender();
        sb.AppendFormat(format, arg0, arg1);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
    }

    [Test]
    public async Task AlignmentComponentGuidDateTime()
    {
        var       format = "{0,10:X}{{0}}{1,-10:c}";
        var       arg0   = Guid.Parse("23ABF120-3BBD-425E-8381-EB88BD39A180");
        var       arg1   = new DateTime(2000, 1, 1).TimeOfDay.Negate();
        using var sb     = new ValueStringAppender();
        sb.AppendFormat(format, arg0, arg1);
        await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
    }

    [Test]
    public async Task AlignmentComponentStringDecimal()
    {
        var format = "{0,10:X}{{0}}{1,-10:c}";
        var names  = new[] { "Adam", "Bridgette", "Carla", "Daniel", "Ebenezer", "Francine", "George" };
        var hours  = new[] { 40, 6.667m, 40.39m, 82, 40.333m, 80, 16.75m };

        for (var i = 0; i < names.Length; i++)
        {
            var arg0 = names[i];
            var arg1 = hours[i];

            using var sb = new ValueStringAppender();
            sb.AppendFormat(format, arg0, arg1);
            await Assert.That(sb.ToString()).IsEqualTo(string.Format(format, arg0, arg1));
        }
    }
}