#if NET8_0_OR_GREATER
using System.Text;

namespace Astra.Text.Tests;

public class ValueStringAppenderTestRune
{
    private static string ReconstructFromRunes(ValueStringAppender.ValueStringAppenderRuneEnumerator e)
    {
        var buf = new StringBuilder();
        while (e.MoveNext()) { buf.Append(e.Current.ToString()); }

        return buf.ToString();
    }

    // ─── EnumerateRunes ───────────────────────────────────────────────────────

    [Test]
    public async Task EnumerateRunesAscii()
    {
        const string testString = "Hello, World!";
        using var    a = new ValueStringAppender(testString);
        await Assert.That(ReconstructFromRunes(a.EnumerateRunes())).IsEqualTo(testString);
    }

    [Test]
    public async Task EnumerateRunesUnicode()
    {
        var testString =
            "\u0030\u0031\u0032\u0033\u0054\u0065\u0073\u0074\u6e2c\u8a66\u1e87\u0353\u031e\u0352\u035f\u0361\u01eb\u0320\u0320\u0309\u030f\u0360\u0361\u0345\u0072\u032c\u033a\u035a\u030d\u035b\u0314\u0352\u0362\u0064\u0320\u034e\u0317\u0333\u0347\u0346\u030b\u030a\u0342\u0350\ud83d\udeb5\ud83c\udffb\u200d\u2640\ufe0f\u0022";

        using var a = new ValueStringAppender(testString);
        await Assert.That(ReconstructFromRunes(a.EnumerateRunes())).IsEqualTo(testString);
    }

    [Test]
    public async Task EnumerateRunesSurrogatePair()
    {
        const string testString = "a\U0001F6B5b"; // 'a', 🚵 (U+1F6B5, surrogate pair), 'b'
        using var    a = new ValueStringAppender(testString);

        var runes = new List<Rune>();
        var e = a.EnumerateRunes();
        while (e.MoveNext()) { runes.Add(e.Current); }

        await Assert.That(runes.Count).IsEqualTo(3); // a, 🚵, b
        await Assert.That(runes[0]).IsEqualTo(new Rune('a'));
        await Assert.That(runes[1]).IsEqualTo(new Rune(0x1F6B5));
        await Assert.That(runes[2]).IsEqualTo(new Rune('b'));
    }

    [Test]
    public async Task EnumerateRunesEmpty()
    {
        using var a = new ValueStringAppender();
        var       e = a.EnumerateRunes();
        await Assert.That(e.MoveNext()).IsFalse();
    }

    // Reset() via IEnumerator boxes the struct, so it resets a copy and not the local variable.
    // Test Reset using a separately obtained enumerator instead.
    [Test]
    public async Task EnumerateRunesReEnumerate()
    {
        const string testString = "Hello";
        using var    a = new ValueStringAppender(testString);

        var pass1 = new List<Rune>();
        var e1 = a.EnumerateRunes();
        while (e1.MoveNext()) { pass1.Add(e1.Current); }

        var pass2 = new List<Rune>();
        var e2 = a.EnumerateRunes();
        while (e2.MoveNext()) { pass2.Add(e2.Current); }

        await Assert.That(pass1.Count).IsEqualTo(pass2.Count);
        for (var i = 0; i < pass1.Count; i++) { await Assert.That(pass1[i]).IsEqualTo(pass2[i]); }
    }

    [Test]
    public async Task EnumerateRunesIEnumerableGetEnumerator()
    {
        const string testString = "Hi";
        using var    a = new ValueStringAppender(testString);

        var runes = new List<Rune>();
        foreach (var rune in a.EnumerateRunes()) { runes.Add(rune); }

        await Assert.That(runes.Count).IsEqualTo(2);
        await Assert.That(runes[0]).IsEqualTo(new Rune('H'));
        await Assert.That(runes[1]).IsEqualTo(new Rune('i'));
    }

    [Test]
    public async Task EnumerateRunesMatchesStringEnumerateRunes()
    {
        const string testString = "0123Test測試ẇord";
        using var    a = new ValueStringAppender(testString);

        var expected = new List<Rune>();
        foreach (var rune in testString.EnumerateRunes()) { expected.Add(rune); }

        var actual = new List<Rune>();
        var e = a.EnumerateRunes();
        while (e.MoveNext()) { actual.Add(e.Current); }

        await Assert.That(actual.Count).IsEqualTo(expected.Count);
        for (var i = 0; i < expected.Count; i++) { await Assert.That(actual[i]).IsEqualTo(expected[i]); }
    }

    // ─── GetRuneAt ────────────────────────────────────────────────────────────

    [Test]
    public async Task GetRuneAtAscii()
    {
        using var a = new ValueStringAppender("Hello");
        await Assert.That(a.GetRuneAt(0)).IsEqualTo(new Rune('H'));
        await Assert.That(a.GetRuneAt(1)).IsEqualTo(new Rune('e'));
        await Assert.That(a.GetRuneAt(4)).IsEqualTo(new Rune('o'));
    }

    [Test]
    public async Task GetRuneAtSurrogatePair()
    {
        const string text = "\U0001F6B5"; // 🚵, encoded as surrogate pair
        using var    a = new ValueStringAppender(text);
        await Assert.That(a.GetRuneAt(0)).IsEqualTo(new Rune(0x1F6B5));
    }

    [Test]
    public async Task GetRuneAtSurrogatePairSecondChar()
    {
        const string text = "a\U0001F6B5b";
        using var    a = new ValueStringAppender(text);
        await Assert.That(a.GetRuneAt(1)).IsEqualTo(new Rune(0x1F6B5));
        await Assert.That(a.GetRuneAt(3)).IsEqualTo(new Rune('b'));
    }

    [Test]
    public void GetRuneAtOutOfRangeThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var a = new ValueStringAppender("Hello");
            _ = a.GetRuneAt(5);
        });
    }

    [Test]
    public void GetRuneAtNegativeThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var a = new ValueStringAppender("Hello");
            _ = a.GetRuneAt(-1);
        });
    }

    [Test]
    public void GetRuneAtLoneSurrogateThrows()
    {
        var       text = new string(new[] { '\uD83D' }); // lone high surrogate
        using var a = new ValueStringAppender(text);
        Assert.Throws<ArgumentException>(() => a.GetRuneAt(0));
    }

    // ─── TryGetRuneAt ─────────────────────────────────────────────────────────

    [Test]
    public async Task TryGetRuneAtAscii()
    {
        using var a = new ValueStringAppender("Hello");
        await Assert.That(a.TryGetRuneAt(0, out var rune)).IsTrue();
        await Assert.That(rune).IsEqualTo(new Rune('H'));
    }

    [Test]
    public async Task TryGetRuneAtSurrogatePair()
    {
        const string text = "\U0001F6B5"; // 🚵
        using var    a = new ValueStringAppender(text);
        await Assert.That(a.TryGetRuneAt(0, out var rune)).IsTrue();
        await Assert.That(rune).IsEqualTo(new Rune(0x1F6B5));
    }

    [Test]
    public async Task TryGetRuneAtLoneSurrogate()
    {
        var       text = new string(new[] { '\uD83D' }); // lone high surrogate
        using var a = new ValueStringAppender(text);
        await Assert.That(a.TryGetRuneAt(0, out _)).IsFalse();
    }

    [Test]
    public void TryGetRuneAtOutOfRangeThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var a = new ValueStringAppender("Hello");
            a.TryGetRuneAt(5, out _);
        });
    }

    [Test]
    public void TryGetRuneAtNegativeThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var a = new ValueStringAppender("Hello");
            a.TryGetRuneAt(-1, out _);
        });
    }

    [Test]
    public async Task TryGetRuneAtAllPositions()
    {
        const string text = "AB\U0001F6B5C"; // A, B, 🚵 (2 chars), C
        using var    a = new ValueStringAppender(text);

        await Assert.That(a.TryGetRuneAt(0, out var r0)).IsTrue();
        await Assert.That(r0).IsEqualTo(new Rune('A'));

        await Assert.That(a.TryGetRuneAt(1, out var r1)).IsTrue();
        await Assert.That(r1).IsEqualTo(new Rune('B'));

        await Assert.That(a.TryGetRuneAt(2, out var r2)).IsTrue();
        await Assert.That(r2).IsEqualTo(new Rune(0x1F6B5));

        await Assert.That(a.TryGetRuneAt(4, out var r4)).IsTrue();
        await Assert.That(r4).IsEqualTo(new Rune('C'));
    }

    // ─── Cross-chunk boundary ─────────────────────────────────────────────────
    // Tests surrogate pair spanning two internal chunk buffers.
    // Uses the real DefaultFixedSize to place the pair at the chunk boundary.

    [Test]
    public async Task TryGetRuneAtCrossChunkBoundary()
    {
        var chunkSize = ValueStringAppender.DefaultFixedSize;

        // Position the high surrogate at the last slot of chunk 0 (chunkSize - 1),
        // low surrogate at the first slot of chunk 1 (chunkSize).
        var prefix = new string('A', chunkSize - 1); // fills chunk 0 up to last slot
        var text = prefix + "\U0001F6B5" + "END";  // pair straddles chunk boundary

        using var a = new ValueStringAppender(text);

        var highSurrogateIndex = chunkSize - 1;
        await Assert.That(a.TryGetRuneAt(highSurrogateIndex, out var rune)).IsTrue();
        await Assert.That(rune).IsEqualTo(new Rune(0x1F6B5));
    }

    [Test]
    public async Task EnumerateRunesCrossChunkBoundary()
    {
        var chunkSize = ValueStringAppender.DefaultFixedSize;
        var prefix = new string('A', chunkSize - 1);
        var testString = prefix + "\U0001F6B5" + "END";

        using var a = new ValueStringAppender(testString);

        var expected = new List<Rune>();
        foreach (var rune in testString.EnumerateRunes()) { expected.Add(rune); }

        var actual = new List<Rune>();
        var e = a.EnumerateRunes();
        while (e.MoveNext()) { actual.Add(e.Current); }

        await Assert.That(actual.Count).IsEqualTo(expected.Count);
        for (var i = 0; i < expected.Count; i++) { await Assert.That(actual[i]).IsEqualTo(expected[i]); }
    }
}
#endif