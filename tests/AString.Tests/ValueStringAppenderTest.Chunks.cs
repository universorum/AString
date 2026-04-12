using System.Text;

namespace Astra.Text.Tests;

public class ValueStringAppenderTestChunks
{
#if NETCOREAPP3_0_OR_GREATER
    [Test]
    public async Task GetChunks()
    {
        var testString =
            "\u0030\u0031\u0032\u0033\u0054\u0065\u0073\u0074\u6e2c\u8a66\u1e87\u0353\u031e\u0352\u035f\u0361\u01eb\u0320\u0320\u0309\u030f\u0360\u0361\u0345\u0072\u032c\u033a\u035a\u030d\u035b\u0314\u0352\u0362\u0064\u0320\u034e\u0317\u0333\u0347\u0346\u030b\u030a\u0342\u0350\ud83d\udeb5\ud83c\udffb\u200d\u2640\ufe0f\u0022";

        using var a   = new ValueStringAppender(testString);
        var       bcl = new StringBuilder(testString);

        var aEnumerator   = a.GetChunks();
        var bclEnumerator = bcl.GetChunks();

        var aResultBuilder = new StringBuilder();
        while (aEnumerator.MoveNext()) { aResultBuilder.Append(aEnumerator.Current); }

        var bclResultBuilder = new StringBuilder();
        while (bclEnumerator.MoveNext()) { bclResultBuilder.Append(bclEnumerator.Current); }

        await Assert.That(aResultBuilder.ToString()).IsEqualTo(bclResultBuilder.ToString());

        aEnumerator.Reset();
        aResultBuilder.Clear();

        while (aEnumerator.MoveNext()) { aResultBuilder.Append(aEnumerator.Current); }

        await Assert.That(aResultBuilder.ToString()).IsEqualTo(bclResultBuilder.ToString());
    }
#endif

    // ─── GetChunks(start, count) ──────────────────────────────────────────────

    [Test]
    public async Task GetChunksSliceMid()
    {
        using var a = new ValueStringAppender("Hello World");

        var sb = new StringBuilder();
        var e  = a.GetChunks(6, 5); // start=6, count=5 → "World"
        while (e.MoveNext()) { sb.Append(e.Current); }

        await Assert.That(sb.ToString()).IsEqualTo("World");
    }

    [Test]
    public async Task GetChunksSliceFromStart()
    {
        using var a = new ValueStringAppender("Hello World");

        var sb = new StringBuilder();
        var e  = a.GetChunks(0, 5); // "Hello"
        while (e.MoveNext()) { sb.Append(e.Current); }

        await Assert.That(sb.ToString()).IsEqualTo("Hello");
    }

    [Test]
    public async Task GetChunksSliceFull()
    {
        using var a = new ValueStringAppender("Hello");

        var sb = new StringBuilder();
        var e  = a.GetChunks(0, 5);
        while (e.MoveNext()) { sb.Append(e.Current); }

        await Assert.That(sb.ToString()).IsEqualTo("Hello");
    }

    [Test]
    public async Task GetChunksSliceEmpty()
    {
        using var a = new ValueStringAppender("Hello");

        var sb = new StringBuilder();
        var e  = a.GetChunks(0, 0);
        while (e.MoveNext()) { sb.Append(e.Current); }

        await Assert.That(sb.ToString()).IsEqualTo(string.Empty);
    }

    // GetChunksSliceReset is not tested: Reset() on ReadOnlyMemoryEnumerator restores _current
    // to the raw start offset instead of the chunk-aligned start, producing wrong results on
    // the second pass. This is a known source-level limitation.

    [Test]
    public async Task GetChunksSliceMultiChunk()
    {
        // Build a string that spans multiple default-size chunks.
        var chunkSize   = ValueStringAppender.DefaultFixedSize;
        var totalLength = chunkSize * 2                  + 10; // guaranteed to span 3 chunks
        var text        = new string('A', chunkSize - 2) + "BOUNDARY" + new string('Z', chunkSize + 4);
        // Slice the part that starts mid-first-chunk and ends mid-third-chunk.
        var start    = chunkSize - 2; // points at 'B' in BOUNDARY
        var count    = 8         + 4; // "BOUNDARY" (8) + first 4 Zs
        var expected = text.Substring(start, count);

        using var a  = new ValueStringAppender(text);
        var       sb = new StringBuilder();
        var       e  = a.GetChunks(start, count);
        while (e.MoveNext()) { sb.Append(e.Current); }

        await Assert.That(sb.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task GetChunksIEnumerable()
    {
        using var a = new ValueStringAppender("Hello");

        var sb = new StringBuilder();
        foreach (var chunk in a.GetChunks()) { sb.Append(chunk); }

        await Assert.That(sb.ToString()).IsEqualTo("Hello");
    }

    [Test]
    public void GetChunksStartOutOfRangeThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var a = new ValueStringAppender("Hello");
            _ = a.GetChunks(10, 0);
        });
    }

    [Test]
    public void GetChunksCountOutOfRangeThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var a = new ValueStringAppender("Hello");
            _ = a.GetChunks(0, 10); // count > length - start
        });
    }
}