using System.Globalization;
using Astra.Text.Models;

namespace Astra.Text.Tests;

public class AStringTest
{
    public enum DuplicateEnum
    {
        A  = 1,
        B  = 2,
        BB = 2,
        C  = 3
    }

    [Flags]
    public enum FlagsEnum
    {
        None = 0,
        Abc  = 1,
        Bcd  = 2,
        Efg  = 4
    }

    public enum StandardEnum { Abc = 1, Def = 2, Ghi = 3 }
    // ─── Concat ────────────────────────────────────────────────────────────────

    [Test]
    public async Task ConcatEnumerableStrings()
    {
        var values = new[] { "Hello", " ", "World" };
        await Assert.That(AString.Concat<string>(values)).IsEqualTo(string.Concat(values));
    }

    [Test]
    public async Task ConcatEnumerableEmpty() { await Assert.That(AString.Concat()).IsEqualTo(string.Empty); }

    [Test]
    public async Task ConcatEnumerableInts()
    {
        var values = new[] { 1, 2, 3 };
        await Assert.That(AString.Concat(values)).IsEqualTo(string.Concat(values));
    }

    [Test]
    public void ConcatEnumerableNullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => AString.Concat<string>(null!));
    }

    [Test]
    [Arguments("Hello", " World")]
    [Arguments("",      "test")]
    [Arguments("abc",   "")]
    [Arguments("",      "")]
    public async Task ConcatTwoSpans(string a, string b)
    {
        await Assert.That(AString.Concat(a.AsSpan(), b.AsSpan())).IsEqualTo(string.Concat(a, b));
    }

    [Test]
    [Arguments("Hello", " ", "World")]
    [Arguments("",      "X", "")]
    public async Task ConcatThreeSpans(string a, string b, string c)
    {
        await Assert.That(AString.Concat(a.AsSpan(), b.AsSpan(), c.AsSpan())).IsEqualTo(string.Concat(a, b, c));
    }

    [Test]
    [Arguments("a",  "b", "c",     "d")]
    [Arguments("",   "",  "",      "")]
    [Arguments("Hi", " ", "World", "!")]
    public async Task ConcatFourSpans(string a, string b, string c, string d)
    {
        await Assert.That(AString.Concat(a.AsSpan(), b.AsSpan(), c.AsSpan(), d.AsSpan()))
            .IsEqualTo(string.Concat(a, b, c, d));
    }

    [Test]
    public async Task ConcatParamsStrings()
    {
        await Assert.That(AString.Concat("Hello", " ",  "World")).IsEqualTo("Hello World");
        await Assert.That(AString.Concat("a",     null, "b")).IsEqualTo(string.Concat("a", null, "b"));
        await Assert.That(AString.Concat()).IsEqualTo(string.Empty);
    }

    // ─── Format – raw format string ────────────────────────────────────────────

    [Test]
    [Arguments("{0}",        42)]
    [Arguments("Value: {0}", 100)]
    [Arguments("abc{0}def",  -1)]
    [Arguments("{0:X}",      255)]
    public async Task FormatOneArg(string format, int arg0)
    {
        await Assert.That(AString.Format(format, arg0)).IsEqualTo(string.Format(format, arg0));
    }

    [Test]
    [Arguments("{0} and {1}", 1,  2)]
    [Arguments("{{0}}{0}{1}", 10, 20)]
    public async Task FormatTwoArgs(string format, int arg0, int arg1)
    {
        await Assert.That(AString.Format(format, arg0, arg1)).IsEqualTo(string.Format(format, arg0, arg1));
    }

    [Test]
    public async Task FormatThreeArgs()
    {
        const string format = "{0}, {1}, {2}";
        await Assert.That(AString.Format(format, 1, 2, 3)).IsEqualTo(string.Format(format, 1, 2, 3));
    }

    [Test]
    public async Task FormatOneArgWithProvider()
    {
        const string format = "{0:F2}";
        const double value  = 3.14159;
        await Assert.That(AString.Format(CultureInfo.InvariantCulture, format, value))
            .IsEqualTo(string.Format(CultureInfo.InvariantCulture, format, value));
    }

    [Test]
    public async Task FormatTwoArgsWithProvider()
    {
        const string format = "{0:F2} {1:N0}";
        await Assert.That(AString.Format(CultureInfo.InvariantCulture, format, 3.14, 1000))
            .IsEqualTo(string.Format(CultureInfo.InvariantCulture, format, 3.14, 1000));
    }

    [Test]
    public async Task FormatThreeArgsWithProvider()
    {
        const string format = "X={0} Y={1} Z={2}";
        await Assert.That(AString.Format(CultureInfo.InvariantCulture, format, "a", "b", "c"))
            .IsEqualTo(string.Format(CultureInfo.InvariantCulture, format, "a", "b", "c"));
    }

    // ─── Format – AStringCompositeFormat ───────────────────────────────────────

    [Test]
    public async Task FormatCompositeOneArg()
    {
        var cf = new AStringCompositeFormat("Hello {0}!");
        await Assert.That(AString.Format(cf, "World")).IsEqualTo("Hello World!");
    }

    [Test]
    public async Task FormatCompositeOneArgWithProvider()
    {
        var cf = new AStringCompositeFormat("{0:F2}");
        await Assert.That(AString.Format(CultureInfo.InvariantCulture, cf, Math.PI))
            .IsEqualTo(Math.PI.ToString("F2", CultureInfo.InvariantCulture));
    }

    [Test]
    public async Task FormatCompositeTwoArgs()
    {
        var cf = new AStringCompositeFormat("{0} + {1} = ?");
        await Assert.That(AString.Format(cf, 1, 2)).IsEqualTo("1 + 2 = ?");
    }

    [Test]
    public async Task FormatCompositeTwoArgsWithProvider()
    {
        var cf = new AStringCompositeFormat("{0:F1} {1:F1}");
        await Assert.That(AString.Format(CultureInfo.InvariantCulture, cf, 1.5, 2.5)).IsEqualTo("1.5 2.5");
    }

    [Test]
    public async Task FormatCompositeThreeArgs()
    {
        var cf = new AStringCompositeFormat("{0}, {1}, {2}");
        await Assert.That(AString.Format(cf, "a", "b", "c")).IsEqualTo("a, b, c");
    }

    [Test]
    public async Task FormatCompositeThreeArgsWithProvider()
    {
        var cf = new AStringCompositeFormat("{0:D} {1:D} {2:D}");
        await Assert.That(AString.Format(CultureInfo.InvariantCulture, cf, 1, 2, 3)).IsEqualTo("1 2 3");
    }

    [Test]
    public async Task FormatCompositeEscapedBraces()
    {
        var cf = new AStringCompositeFormat("{{{0}}}");
        await Assert.That(AString.Format(cf, "x")).IsEqualTo("{x}");
    }

    [Test]
    public void FormatCompositeNullFormatThrows()
    {
        Assert.Throws<ArgumentNullException>(() => _ = AString.Format((AStringCompositeFormat)null!, 42));
    }

    // ─── Join ──────────────────────────────────────────────────────────────────

    [Test]
    [Arguments(',', new[] { 1, 2, 3 })]
    [Arguments('-', new[] { 10, 20 })]
    [Arguments(',', new int[0])]
    public async Task JoinCharIEnumerable(char sep, int[] values)
    {
        await Assert.That(AString.Join(sep, (IEnumerable<int>)values)).IsEqualTo(string.Join(sep, values));
    }

    [Test]
    [Arguments(',', new[] { "a", "b", "c" })]
    [Arguments('-', new[] { "hello", "world" })]
    public async Task JoinCharIEnumerableStrings(char sep, string[] values)
    {
        await Assert.That(AString.Join(sep, (IEnumerable<string>)values)).IsEqualTo(string.Join(sep, values));
    }

    [Test]
    public async Task JoinCharSpan()
    {
        ReadOnlySpan<int> values = [1, 2, 3];
        await Assert.That(AString.Join(',', values)).IsEqualTo(string.Join(',', values.ToArray()));
    }

    [Test]
    public async Task JoinCharSpanEmpty()
    {
        ReadOnlySpan<int> values = [];
        await Assert.That(AString.Join(',', values)).IsEqualTo(string.Empty);
    }

    [Test]
    [Arguments(", ", new[] { "a", "b", "c" })]
    [Arguments("::", new[] { "x" })]
    [Arguments("--", new string[0])]
    public async Task JoinStringIEnumerable(string sep, string[] values)
    {
        await Assert.That(AString.Join(sep, (IEnumerable<string>)values)).IsEqualTo(string.Join(sep, values));
    }

    [Test]
    public async Task JoinStringSpan()
    {
        ReadOnlySpan<int> values = [1, 2, 3];
        await Assert.That(AString.Join(", ", values)).IsEqualTo(string.Join(", ", values.ToArray()));
    }

    [Test]
    public async Task JoinCharSpanWithStartAndCount()
    {
        ReadOnlySpan<int?> values = [1, 2, 3, 4, 5];
        var                result = AString.Join(',', values, 1, 3);
        await Assert.That(result).IsEqualTo("2,3,4");
    }

    [Test]
    public async Task JoinStringSpanWithStartAndCount()
    {
        ReadOnlySpan<string?> values = ["a", "b", "c", "d"];
        var                   result = AString.Join(", ", values, 0, 2);
        await Assert.That(result).IsEqualTo("a, b");
    }

    [Test]
    public async Task JoinStringSpanWithStartAndCountFull()
    {
        ReadOnlySpan<string?> values = ["x", "y", "z"];
        var                   result = AString.Join("-", values, 0, 3);
        await Assert.That(result).IsEqualTo("x-y-z");
    }

    [Test]
    public async Task Duplicate()
    {
        await Assert.That(AString.Format("{0}", DuplicateEnum.A)).IsEqualTo("A");
        await Assert.That(AString.Format("{0}", DuplicateEnum.B)).IsEqualTo("B");
        await Assert.That(AString.Format("{0}", DuplicateEnum.BB)).IsEqualTo("B");
        await Assert.That(AString.Format("{0}", DuplicateEnum.C)).IsEqualTo("C");
    }

    [Test]
    public async Task Standard()
    {
        await Assert.That(AString.Format("{0}", StandardEnum.Abc)).IsEqualTo("Abc");
        await Assert.That(AString.Format("{0}", StandardEnum.Def)).IsEqualTo("Def");
        await Assert.That(AString.Format("{0}", StandardEnum.Ghi)).IsEqualTo("Ghi");
    }

    [Test]
    public async Task Flags()
    {
        await Assert.That(AString.Format("{0}", FlagsEnum.Abc | FlagsEnum.Bcd))
            .IsEqualTo($"{FlagsEnum.Abc | FlagsEnum.Bcd}");
        await Assert.That(AString.Format("{0}", FlagsEnum.None)).IsEqualTo("None");
        await Assert.That(AString.Format("{0}", FlagsEnum.Efg)).IsEqualTo("Efg");
    }
}