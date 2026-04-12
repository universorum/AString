using System.Text;
using Astra.Text.Models;

namespace Astra.Text.Tests;

public class AStringCompositeFormatTest
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
        Assert.Throws<FormatException>(() => { _ = new AStringCompositeFormat(format); });
    }

    // ─── Constructor / properties ─────────────────────────────────────────────

    [Test]
    public async Task FormatPropertyReturnsOriginalString()
    {
        const string fmt = "Hello {0}!";
        var          cf  = new AStringCompositeFormat(fmt);
        await Assert.That(cf.Format).IsEqualTo(fmt);
    }

    [Test]
    public async Task MinimumArgumentCountNoArgs()
    {
        var cf = new AStringCompositeFormat("no placeholders here");
        await Assert.That(cf.MinimumArgumentCount).IsZero();
    }

    [Test]
    public async Task MinimumArgumentCountOneArgSegment()
    {
        var cf = new AStringCompositeFormat("{0}");
        await Assert.That(cf.MinimumArgumentCount).IsEqualTo(1);
    }

    [Test]
    public async Task MinimumArgumentCountTwoArgSegments()
    {
        var cf = new AStringCompositeFormat("{0} and {1}");
        await Assert.That(cf.MinimumArgumentCount).IsEqualTo(2);
    }

    [Test]
    public async Task MinimumArgumentCountThreeArgSegments()
    {
        var cf = new AStringCompositeFormat("{0}{1}{2}");
        await Assert.That(cf.MinimumArgumentCount).IsEqualTo(3);
    }

    [Test]
    public async Task MinimumArgumentCountCountsRepeatedSegments()
    {
        // {0} appears twice → two argument segments are counted
        var cf = new AStringCompositeFormat("{0} = {0}");
        await Assert.That(cf.MinimumArgumentCount).IsEqualTo(1);
    }

    [Test]
    public async Task MinimumArgumentCountIgnoresEscapedBraces()
    {
        var cf = new AStringCompositeFormat("{{literal}} {0}");
        await Assert.That(cf.MinimumArgumentCount).IsEqualTo(1);
    }

    [Test]
    public async Task MinimumArgumentCountOnlyEscapedBraces()
    {
        var cf = new AStringCompositeFormat("{{}}");
        await Assert.That(cf.MinimumArgumentCount).IsZero();
    }

    [Test]
    public async Task MinimumArgumentCountWithFormatAndAlignment()
    {
        var cf = new AStringCompositeFormat("{0,10:F2} {1,-5:X}");
        await Assert.That(cf.MinimumArgumentCount).IsEqualTo(2);
    }

    [Test]
    public void ConstructorNullThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new AStringCompositeFormat(null!));
    }

    // ─── Usage through ValueStringBuilder ────────────────────────────────────

    [Test]
    public async Task AppendFormatOneArg()
    {
        var       cf = new AStringCompositeFormat("Hello {0}!");
        using var sb = new ValueStringBuilder();
        sb.AppendFormat(provider: null, cf, "World");
        await Assert.That(sb.ToString()).IsEqualTo("Hello World!");
    }

    [Test]
    public async Task AppendFormatTwoArgs()
    {
        var       cf = new AStringCompositeFormat("{0} + {1}");
        using var sb = new ValueStringBuilder();
        sb.AppendFormat(provider: null, cf, 1, 2);
        await Assert.That(sb.ToString()).IsEqualTo("1 + 2");
    }

    [Test]
    public async Task AppendFormatThreeArgs()
    {
        var       cf = new AStringCompositeFormat("{0}, {1}, {2}");
        using var sb = new ValueStringBuilder();
        sb.AppendFormat(null, cf, "a", "b", "c");
        await Assert.That(sb.ToString()).IsEqualTo("a, b, c");
    }

    [Test]
    public async Task AppendFormatSpanArgs()
    {
        var                   cf   = new AStringCompositeFormat("{0} {1}");
        using var             sb   = new ValueStringBuilder();
        ReadOnlySpan<string?> args = ["hello", "world"];
        sb.AppendFormat<string?>(null, cf, args);
        await Assert.That(sb.ToString()).IsEqualTo("hello world");
    }

    [Test]
    public void AppendFormatNullCompositeFormatThrows()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            using var sb = new ValueStringBuilder();
            sb.AppendFormat(provider: null, (AStringCompositeFormat)null!, 42);
        });
    }

    [Test]
    public async Task AppendFormatEmptyFormat()
    {
        var       cf = new AStringCompositeFormat("");
        using var sb = new ValueStringBuilder();
        sb.AppendFormat(provider: null, cf, 42);
        await Assert.That(sb.ToString()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task AppendFormatLiteralOnly()
    {
        var       cf = new AStringCompositeFormat("no args here");
        using var sb = new ValueStringBuilder();
        sb.AppendFormat(provider: null, cf, 0);
        await Assert.That(sb.ToString()).IsEqualTo("no args here");
    }

    [Test]
    public async Task AppendFormatEscapedBraces()
    {
        var       cf = new AStringCompositeFormat("{{{0}}}");
        using var sb = new ValueStringBuilder();
        sb.AppendFormat(provider: null, cf, "x");
        await Assert.That(sb.ToString()).IsEqualTo("{x}");
    }

    // ─── CreateString / CreateUtf8Array ───────────────────────────────────────

    // Reusable static selectors (RequireStaticDelegate is enforced by the API)

#if NET8_0_OR_GREATER
    private static void StringArraySelector(ref AStringCompositeFormat.ParameterSender s, int i, string?[] args) =>
        s.Send(args[i]);

    private static void ObjectArraySelector(ref AStringCompositeFormat.ParameterSender s, int i, object?[] args) =>
        s.Send(args[i]);

    private static void IntArraySelector(ref AStringCompositeFormat.ParameterSender s, int i, int[] args) =>
        s.Send(args[i]);

    private static void DoubleArraySelector(ref AStringCompositeFormat.ParameterSender s, int i, double[] args) =>
        s.Send(args[i]);

    // ── Normal (literal) segment ──────────────────────────────────────────────

    [Test]
    public async Task CreateString_EmptyFormat_ReturnsEmpty()
    {
        var cf = new AStringCompositeFormat("");
        var result = cf.CreateString(static (ref s, i, a) => s.Send(a[i]), Array.Empty<string?>());
        await Assert.That(result).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task CreateString_LiteralOnly_ReturnsLiteral()
    {
        var cf = new AStringCompositeFormat("hello world");
        var result = cf.CreateString(static (ref s, i, a) => s.Send(a[i]), Array.Empty<string?>());
        await Assert.That(result).IsEqualTo("hello world");
    }

    // ── EscapedOpenBracket segment ────────────────────────────────────────────

    [Test]
    public async Task CreateString_EscapedOpenBracket_ProducesOpenBrace()
    {
        var cf = new AStringCompositeFormat("{{");
        var result = cf.CreateString(static (ref s, i, a) => s.Send(a[i]), Array.Empty<string?>());
        await Assert.That(result).IsEqualTo("{");
    }

    // ── EscapedCloseBracket segment ───────────────────────────────────────────

    [Test]
    public async Task CreateString_EscapedCloseBracket_ProducesCloseBrace()
    {
        var cf = new AStringCompositeFormat("}}");
        var result = cf.CreateString(static (ref s, i, a) => s.Send(a[i]), Array.Empty<string?>());
        await Assert.That(result).IsEqualTo("}");
    }

    // ── Argument segment – basic ──────────────────────────────────────────────

    [Test]
    public async Task CreateString_SingleArgument_InsertsValue()
    {
        var cf = new AStringCompositeFormat("Hello {0}!");
        var result = cf.CreateString(StringArraySelector, new[] { "World" });
        await Assert.That(result).IsEqualTo("Hello World!");
    }

    [Test]
    public async Task CreateString_TwoArguments_InsertsValues()
    {
        var cf = new AStringCompositeFormat("{0} + {1}");
        var result = cf.CreateString(IntArraySelector, new[] { 1, 2 });
        await Assert.That(result).IsEqualTo("1 + 2");
    }

    [Test]
    public async Task CreateString_ThreeArguments_InsertsValues()
    {
        var cf = new AStringCompositeFormat("{0}, {1}, {2}");
        var result = cf.CreateString(StringArraySelector, new[] { "a", "b", "c" });
        await Assert.That(result).IsEqualTo("a, b, c");
    }

    [Test]
    public async Task CreateString_RepeatedArgument_InsertsTwice()
    {
        var cf = new AStringCompositeFormat("{0} = {0}");
        var result = cf.CreateString(IntArraySelector, new[] { 42 });
        await Assert.That(result).IsEqualTo("42 = 42");
    }

    // ── Argument segment – null value ─────────────────────────────────────────

    [Test]
    public async Task CreateString_NullArgument_ProducesEmpty()
    {
        var cf = new AStringCompositeFormat("[{0}]");
        var result = cf.CreateString(StringArraySelector, new string?[] { null });
        await Assert.That(result).IsEqualTo("[]");
    }

    // ── Argument segment – format specifier ──────────────────────────────────

    [Test]
    public async Task CreateString_FormatSpecifier_AppliesFormat()
    {
        var cf = new AStringCompositeFormat("{0:X}");
        var result = cf.CreateString(IntArraySelector, new[] { 255 });
        var expected = string.Format("{0:X}", 255);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task CreateString_FormatSpecifierF2_AppliesFormat()
    {
        var cf = new AStringCompositeFormat("{0:F2}");
        var result = cf.CreateString(DoubleArraySelector, new[] { Math.PI });
        var expected = string.Format("{0:F2}", Math.PI);
        await Assert.That(result).IsEqualTo(expected);
    }

    // ── Argument segment – alignment ──────────────────────────────────────────

    [Test]
    public async Task CreateString_PositiveAlignment_RightAligns()
    {
        var cf = new AStringCompositeFormat("{0,5}");
        var result = cf.CreateString(IntArraySelector, new[] { 42 });
        var expected = string.Format("{0,5}", 42);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task CreateString_NegativeAlignment_LeftAligns()
    {
        var cf = new AStringCompositeFormat("{0,-5}");
        var result = cf.CreateString(IntArraySelector, new[] { 42 });
        var expected = string.Format("{0,-5}", 42);
        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task CreateString_AlignmentAndFormatSpecifier_AppliesBoth()
    {
        var cf = new AStringCompositeFormat("{0,10:F2}");
        var result = cf.CreateString(DoubleArraySelector, new[] { Math.PI });
        var expected = string.Format("{0,10:F2}", Math.PI);
        await Assert.That(result).IsEqualTo(expected);
    }

    // ── Mixed segments ────────────────────────────────────────────────────────

    [Test]
    public async Task CreateString_MixedEscapesAndArguments_ProducesCorrectOutput()
    {
        // {{{0}}} → {<value>}
        var cf = new AStringCompositeFormat("{{{0}}}");
        var result = cf.CreateString(StringArraySelector, new[] { "x" });
        await Assert.That(result).IsEqualTo("{x}");
    }

    [Test]
    public async Task CreateString_AllSegmentTypes_ProducesCorrectOutput()
    {
        // literal + {{ + arg + }} + literal
        var cf = new AStringCompositeFormat("pre{{mid{0}end}}post");
        var result = cf.CreateString(StringArraySelector, new[] { "val" });
        var expected = string.Format("pre{{mid{0}end}}post", "val");
        await Assert.That(result).IsEqualTo(expected);
    }

    // ── Unicode content ───────────────────────────────────────────────────────

    [Test]
    public async Task CreateString_UnicodeInFormat_HandledCorrectly()
    {
        var cf = new AStringCompositeFormat("\u30cf\u30fc\u30c8: {0}");
        var result = cf.CreateString(StringArraySelector, new[] { "\u2764" });
        var expected = "\u30cf\u30fc\u30c8: \u2764";
        await Assert.That(result).IsEqualTo(expected);
    }

    // ── Selector exception wrapping ───────────────────────────────────────────

    [Test]
    public void CreateString_SelectorThrows_WrapsInFormatException()
    {
        var cf = new AStringCompositeFormat("{0}");
        Assert.Throws<FormatException>(() =>
            cf.CreateString(static (ref s, i, a) => throw new InvalidOperationException("boom"), new[] { 42 }));
    }

    [Test]
    public void CreateString_SelectorThrowsForSecondArg_WrapsInFormatException()
    {
        var cf = new AStringCompositeFormat("{0} {1}");
        Assert.Throws<FormatException>(() => cf.CreateString(static (ref s, i, a) =>
            {
                if (i == 0)
                {
                    s.Send(a[i]);
                    return;
                }

                throw new OverflowException("second arg explodes");
            },
            new[] { 1, 2 }));
    }

    // ── CreateUtf8Array mirrors CreateString ──────────────────────────────────

    [Test]
    public async Task CreateUtf8Array_EmptyFormat_ReturnsEmptyArray()
    {
        var cf = new AStringCompositeFormat("");
        var result = cf.CreateUtf8Array(static (ref s, i, a) => s.Send(a[i]), Array.Empty<string?>());
        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task CreateUtf8Array_LiteralOnly_ReturnsUtf8Bytes()
    {
        var cf = new AStringCompositeFormat("hello");
        var result = cf.CreateUtf8Array(static (ref s, i, a) => s.Send(a[i]), Array.Empty<string?>());
        var expected = "hello"u8.ToArray();
        await Assert.That(result).IsEquivalentTo(expected);
    }

    [Test]
    public async Task CreateUtf8Array_SingleArgument_MatchesCreateString()
    {
        var cf = new AStringCompositeFormat("Hello {0}!");
        var str = cf.CreateString(StringArraySelector, new[] { "World" });
        var utf8 = cf.CreateUtf8Array(StringArraySelector, new[] { "World" });
        var encoded = Encoding.UTF8.GetBytes(str);
        await Assert.That(utf8).IsEquivalentTo(encoded);
    }

    [Test]
    public async Task CreateUtf8Array_EscapedBraces_MatchesCreateString()
    {
        var cf = new AStringCompositeFormat("{{{0}}}");
        var str = cf.CreateString(StringArraySelector, new[] { "val" });
        var utf8 = cf.CreateUtf8Array(StringArraySelector, new[] { "val" });
        var encoded = Encoding.UTF8.GetBytes(str);
        await Assert.That(utf8).IsEquivalentTo(encoded);
    }

    [Test]
    public async Task CreateUtf8Array_UnicodeContent_MatchesCreateString()
    {
        var cf = new AStringCompositeFormat("\u30cf\u30fc\u30c8: {0}");
        var arg = "\uD83D\uDC4D"; // thumbs-up emoji (surrogate pair)
        var str = cf.CreateString(StringArraySelector, new[] { arg });
        var utf8 = cf.CreateUtf8Array(StringArraySelector, new[] { arg });
        var encoded = Encoding.UTF8.GetBytes(str);
        await Assert.That(utf8).IsEquivalentTo(encoded);
    }

    [Test]
    public async Task CreateUtf8Array_AlignmentAndFormat_MatchesCreateString()
    {
        var cf = new AStringCompositeFormat("{0,10:F2} {1,-5:X}");
        var str = cf.CreateString(IntArraySelector, new[] { 255, 255 });
        var utf8 = cf.CreateUtf8Array(IntArraySelector, new[] { 255, 255 });
        var encoded = Encoding.UTF8.GetBytes(str);
        await Assert.That(utf8).IsEquivalentTo(encoded);
    }

    [Test]
    public void CreateUtf8Array_SelectorThrows_WrapsInFormatException()
    {
        var cf = new AStringCompositeFormat("{0}");
        Assert.Throws<FormatException>(() =>
            cf.CreateUtf8Array(static (ref s, i, a) => throw new InvalidOperationException("boom"), new[] { 42 }));
    }
#endif
}