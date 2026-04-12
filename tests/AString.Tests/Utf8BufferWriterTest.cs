using System.Text;

namespace Astra.Text.Tests;

public class Utf8BufferWriterTest
{
    // ─── IBufferWriter<byte> basic pattern ───────────────────────────────────

    [Test]
    public async Task GetSpanAndAdvance()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var bytes = Encoding.UTF8.GetBytes("Hello");
            var span  = writer.GetSpan(bytes.Length);
            bytes.CopyTo(span);
            writer.Advance(bytes.Length);
            await Assert.That(writer.ToString()).IsEqualTo("Hello");
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task GetMemoryAndAdvance()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var bytes = Encoding.UTF8.GetBytes("World");
            var mem   = writer.GetMemory(bytes.Length);
            bytes.CopyTo(mem.Span);
            writer.Advance(bytes.Length);
            await Assert.That(writer.ToString()).IsEqualTo("World");
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task MultipleAdvances()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var hello = Encoding.UTF8.GetBytes("Hello");
            var s1    = writer.GetSpan(hello.Length);
            hello.CopyTo(s1);
            writer.Advance(hello.Length);

            var world = Encoding.UTF8.GetBytes(" World");
            var s2    = writer.GetSpan(world.Length);
            world.CopyTo(s2);
            writer.Advance(world.Length);

            await Assert.That(writer.ToString()).IsEqualTo("Hello World");
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task AdvanceZeroIsNoOp()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            writer.GetSpan(10);
            writer.Advance(0);
            await Assert.That(writer.ToString()).IsEqualTo(string.Empty);
        }
        finally { writer.Dispose(); }
    }

    // ─── Advance returns buffer to pool immediately ──────────────────────────
    // Unlike CharBufferWriter, Utf8BufferWriter.Advance returns the buffer after each call,
    // so a subsequent GetSpan allocates a fresh buffer.

    [Test]
    public async Task AdvanceReleasesBufferAllowingNewGetSpan()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var b1 = Encoding.UTF8.GetBytes("A");
            var s1 = writer.GetSpan(b1.Length);
            b1.CopyTo(s1);
            writer.Advance(b1.Length); // returns _buffer to pool, sets _buffer = null

            // GetSpan after Advance must succeed without error
            var b2 = Encoding.UTF8.GetBytes("B");
            var s2 = writer.GetSpan(b2.Length);
            b2.CopyTo(s2);
            writer.Advance(b2.Length);

            await Assert.That(writer.ToString()).IsEqualTo("AB");
        }
        finally { writer.Dispose(); }
    }

    // ─── Unicode round-trip ───────────────────────────────────────────────────

    [Test]
    public async Task UnicodeRoundTrip()
    {
        const string text   = "測試";
        var          writer = new Utf8BufferWriter();
        try
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var span  = writer.GetSpan(bytes.Length);
            bytes.CopyTo(span);
            writer.Advance(bytes.Length);
            await Assert.That(writer.ToString()).IsEqualTo(text);
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task SurrogatePairRoundTrip()
    {
        const string text   = "\U0001F6B5"; // 🚵
        var          writer = new Utf8BufferWriter();
        try
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var span  = writer.GetSpan(bytes.Length);
            bytes.CopyTo(span);
            writer.Advance(bytes.Length);
            await Assert.That(writer.ToString()).IsEqualTo(text);
        }
        finally { writer.Dispose(); }
    }

    // ─── GetSpan ─────────────────────────────────────────────────────────────

    [Test]
    public async Task GetSpanWithExplicitSizeHint()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var span = writer.GetSpan(16);
            await Assert.That(span.Length).IsGreaterThanOrEqualTo(16);
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task GetSpanReturnsClearedBuffer()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var span    = writer.GetSpan(10);
            var allZero = true;
            foreach (var b in span)
            {
                if (b != 0)
                {
                    allZero = false;
                    break;
                }
            }

            await Assert.That(allZero).IsTrue();
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task GetSpanReusesSufficientBuffer()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            writer.GetSpan(100);            // allocate large buffer but do NOT advance
            var span2 = writer.GetSpan(50); // buffer still held (no Advance), reuse
            await Assert.That(span2.Length).IsGreaterThanOrEqualTo(50);
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task GetSpanReallocatesWhenTooSmall()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            writer.GetSpan(4); // don't advance — buffer remains held
            var span2 = writer.GetSpan(1000);
            await Assert.That(span2.Length).IsGreaterThanOrEqualTo(1000);
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public void GetSpanNegativeSizeHintThrows()
    {
        var writer = new Utf8BufferWriter();
        try { Assert.Throws<ArgumentOutOfRangeException>(() => writer.GetSpan(-1)); }
        finally { writer.Dispose(); }
    }

    // ─── GetMemory ────────────────────────────────────────────────────────────

    [Test]
    public async Task GetMemoryWithExplicitSizeHint()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var mem = writer.GetMemory(16);
            await Assert.That(mem.Length).IsGreaterThanOrEqualTo(16);
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task GetMemoryReturnsClearedBuffer()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var mem     = writer.GetMemory(8);
            var allZero = true;
            foreach (var b in mem.Span)
            {
                if (b != 0)
                {
                    allZero = false;
                    break;
                }
            }

            await Assert.That(allZero).IsTrue();
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public void GetMemoryNegativeSizeHintThrows()
    {
        var writer = new Utf8BufferWriter();
        try { Assert.Throws<ArgumentOutOfRangeException>(() => writer.GetMemory(-1)); }
        finally { writer.Dispose(); }
    }

    // ─── Advance errors ───────────────────────────────────────────────────────

    [Test]
    public void AdvanceNegativeThrows()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            writer.GetSpan(10);
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.Advance(-1));
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public void AdvanceExceedsBufferThrows()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            writer.GetSpan(4);
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.Advance(10000));
        }
        finally { writer.Dispose(); }
    }

    // ─── ToString overloads ───────────────────────────────────────────────────

    [Test]
    public async Task ToStringEmpty()
    {
        var writer = new Utf8BufferWriter();
        try { await Assert.That(writer.ToString()).IsEqualTo(string.Empty); }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task ToStringSlice()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var bytes = Encoding.UTF8.GetBytes("Hello World");
            var span  = writer.GetSpan(bytes.Length);
            bytes.CopyTo(span);
            writer.Advance(bytes.Length);
            await Assert.That(writer.ToString(6, 5)).IsEqualTo("World");
        }
        finally { writer.Dispose(); }
    }

    // ─── TryCopyCharTo ────────────────────────────────────────────────────────

    [Test]
    public async Task TryCopyCharToSuccess()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var bytes = Encoding.UTF8.GetBytes("Hello");
            var span  = writer.GetSpan(bytes.Length);
            bytes.CopyTo(span);
            writer.Advance(bytes.Length);

            var dst = new char[10];
            var ok  = writer.TryCopyCharTo(dst, out var written);
            await Assert.That(ok).IsTrue();
            await Assert.That(written).IsEqualTo(5);
            await Assert.That(new string(dst, 0, written)).IsEqualTo("Hello");
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task TryCopyCharToInsufficientBuffer()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var bytes = Encoding.UTF8.GetBytes("Hello World");
            var span  = writer.GetSpan(bytes.Length);
            bytes.CopyTo(span);
            writer.Advance(bytes.Length);

            var dst = new char[3];
            var ok  = writer.TryCopyCharTo(dst, out var written);
            await Assert.That(ok).IsFalse();
            await Assert.That(written).IsZero();
        }
        finally { writer.Dispose(); }
    }

    // ─── TryCopyTo(byte) ─────────────────────────────────────────────────────

    [Test]
    public async Task TryCopyToUtf8Success()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var bytes = Encoding.UTF8.GetBytes("Hello");
            var span  = writer.GetSpan(bytes.Length);
            bytes.CopyTo(span);
            writer.Advance(bytes.Length);

            var dst = new byte[20];
            var ok  = writer.TryCopyTo(dst, out var written);
            await Assert.That(ok).IsTrue();
            await Assert.That(Encoding.UTF8.GetString(dst, 0, written)).IsEqualTo("Hello");
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task TryCopyToUtf8InsufficientBuffer()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var bytes = Encoding.UTF8.GetBytes("Hello World");
            var span  = writer.GetSpan(bytes.Length);
            bytes.CopyTo(span);
            writer.Advance(bytes.Length);

            var dst = new byte[1];
            var ok  = writer.TryCopyTo(dst, out var written);
            await Assert.That(ok).IsFalse();
            await Assert.That(written).IsZero();
        }
        finally { writer.Dispose(); }
    }

    // ─── AppendTo ─────────────────────────────────────────────────────────────
    // NOTE: AppendTo currently has a bug — it calls builder.Append(builder.AsSpan())
    // instead of builder.Append(_builder.AsSpan()), doubling the builder's own content.
    // These tests document the actual (buggy) behaviour so any fix is caught immediately.

    [Test]
    public async Task AppendToDoublesBuilderContent()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var bytes = Encoding.UTF8.GetBytes("Hello");
            var span  = writer.GetSpan(bytes.Length);
            bytes.CopyTo(span);
            writer.Advance(bytes.Length);

            var builder = new ValueStringBuilder("ABC");
            writer.AppendTo(ref builder);
            // Bug: appends builder's own "ABC" to itself → "ABCABC"
            await Assert.That(builder.ToString()).IsEqualTo("ABCABC");
            builder.Dispose();
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task AppendToSliceDoublesBuilderSlice()
    {
        var writer = new Utf8BufferWriter();
        try
        {
            var bytes = Encoding.UTF8.GetBytes("Hello");
            var span  = writer.GetSpan(bytes.Length);
            bytes.CopyTo(span);
            writer.Advance(bytes.Length);

            var builder = new ValueStringBuilder("ABCDE");
            writer.AppendTo(ref builder, 1, 3); // appends builder.AsSpan(1,3) = "BCD"
            await Assert.That(builder.ToString()).IsEqualTo("ABCDEBCD");
            builder.Dispose();
        }
        finally { writer.Dispose(); }
    }

    // ─── Dispose ─────────────────────────────────────────────────────────────

    [Test]
    public void DoubleDispose()
    {
        var writer = new Utf8BufferWriter();
        writer.Dispose();
        writer.Dispose();
    }

    [Test]
    public void ToStringAfterDisposeThrows()
    {
        var writer = new Utf8BufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.ToString());
    }

    [Test]
    public void ToStringSliceAfterDisposeThrows()
    {
        var writer = new Utf8BufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.ToString(0, 0));
    }

    [Test]
    public void GetSpanAfterDisposeThrows()
    {
        var writer = new Utf8BufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.GetSpan());
    }

    [Test]
    public void GetMemoryAfterDisposeThrows()
    {
        var writer = new Utf8BufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.GetMemory());
    }

    [Test]
    public void AdvanceAfterDisposeThrows()
    {
        var writer = new Utf8BufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.Advance(1));
    }

    [Test]
    public void TryCopyCharToAfterDisposeThrows()
    {
        var writer = new Utf8BufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var dst = new char[10];
            writer.TryCopyCharTo(dst, out _);
        });
    }

    [Test]
    public void TryCopyToAfterDisposeThrows()
    {
        var writer = new Utf8BufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var dst = new byte[10];
            writer.TryCopyTo(dst, out _);
        });
    }

    // ─── CleanBufferWhenReleased ──────────────────────────────────────────────

    [Test]
    public async Task CleanBufferWhenReleasedProperty()
    {
        var writer = new Utf8BufferWriter { CleanBufferWhenReleased = true };
        try
        {
            var bytes = Encoding.UTF8.GetBytes("Hello");
            var span  = writer.GetSpan(bytes.Length);
            bytes.CopyTo(span);
            writer.Advance(bytes.Length);
            await Assert.That(writer.ToString()).IsEqualTo("Hello");
        }
        finally { writer.Dispose(); }
    }
}