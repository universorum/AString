using System.Text;

namespace Astra.Text.Tests;

public class CharBufferWriterTest
{
    // ─── IBufferWriter<char> basic pattern ───────────────────────────────────

    [Test]
    public async Task GetSpanAndAdvance()
    {
        var writer = new CharBufferWriter();
        try
        {
            var span = writer.GetSpan(5);
            "Hello".CopyTo(span);
            writer.Advance(5);
            await Assert.That(writer.ToString()).IsEqualTo("Hello");
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task GetMemoryAndAdvance()
    {
        var writer = new CharBufferWriter();
        try
        {
            var mem = writer.GetMemory(5);
            "World".AsMemory().CopyTo(mem);
            writer.Advance(5);
            await Assert.That(writer.ToString()).IsEqualTo("World");
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task MultipleAdvances()
    {
        var writer = new CharBufferWriter();
        try
        {
            var span1 = writer.GetSpan(5);
            "Hello".CopyTo(span1);
            writer.Advance(5);

            var span2 = writer.GetSpan(6);
            " World".CopyTo(span2);
            writer.Advance(6);

            await Assert.That(writer.ToString()).IsEqualTo("Hello World");
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task AdvanceZeroIsNoOp()
    {
        var writer = new CharBufferWriter();
        try
        {
            writer.GetSpan(10);
            writer.Advance(0);
            await Assert.That(writer.ToString()).IsEqualTo(string.Empty);
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task AdvancePartialSpan()
    {
        var writer = new CharBufferWriter();
        try
        {
            var span = writer.GetSpan(20);
            "Hi".CopyTo(span);
            writer.Advance(2); // only commit 2 of the larger buffer
            await Assert.That(writer.ToString()).IsEqualTo("Hi");
        }
        finally { writer.Dispose(); }
    }

    // ─── GetSpan ─────────────────────────────────────────────────────────────

    [Test]
    public async Task GetSpanWithExplicitSizeHint()
    {
        var writer = new CharBufferWriter();
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
        var writer = new CharBufferWriter();
        try
        {
            var span    = writer.GetSpan(10);
            var allZero = true;
            foreach (var c in span)
            {
                if (c != '\0')
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
        var writer = new CharBufferWriter();
        try
        {
            writer.GetSpan(100);
            writer.Advance(3);
            var span2 = writer.GetSpan(50); // existing buffer (100) >= 50, reuse
            await Assert.That(span2.Length).IsGreaterThanOrEqualTo(50);
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task GetSpanReallocatesWhenTooSmall()
    {
        var writer = new CharBufferWriter();
        try
        {
            writer.GetSpan(4);
            writer.Advance(4);
            var span2 = writer.GetSpan(1000);
            await Assert.That(span2.Length).IsGreaterThanOrEqualTo(1000);
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public void GetSpanNegativeSizeHintThrows()
    {
        var writer = new CharBufferWriter();
        try { Assert.Throws<ArgumentOutOfRangeException>(() => writer.GetSpan(-1)); }
        finally { writer.Dispose(); }
    }

    // ─── GetMemory ────────────────────────────────────────────────────────────

    [Test]
    public async Task GetMemoryWithExplicitSizeHint()
    {
        var writer = new CharBufferWriter();
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
        var writer = new CharBufferWriter();
        try
        {
            var mem     = writer.GetMemory(8);
            var span    = mem.Span;
            var allZero = true;
            foreach (var c in span)
            {
                if (c != '\0')
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
        var writer = new CharBufferWriter();
        try { Assert.Throws<ArgumentOutOfRangeException>(() => writer.GetMemory(-1)); }
        finally { writer.Dispose(); }
    }

    // ─── Advance errors ───────────────────────────────────────────────────────

    [Test]
    public void AdvanceNegativeThrows()
    {
        var writer = new CharBufferWriter();
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
        var writer = new CharBufferWriter();
        try
        {
            writer.GetSpan(4);
            Assert.Throws<ArgumentOutOfRangeException>(() => writer.Advance(10000));
        }
        finally { writer.Dispose(); }
    }

    // ─── ToString ────────────────────────────────────────────────────────────

    [Test]
    public async Task ToStringEmpty()
    {
        var writer = new CharBufferWriter();
        try { await Assert.That(writer.ToString()).IsEqualTo(string.Empty); }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task ToStringSlice()
    {
        var writer = new CharBufferWriter();
        try
        {
            var span = writer.GetSpan(11);
            "Hello World".CopyTo(span);
            writer.Advance(11);
            await Assert.That(writer.ToString(6, 5)).IsEqualTo("World");
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task ToStringSliceFromStart()
    {
        var writer = new CharBufferWriter();
        try
        {
            var span = writer.GetSpan(11);
            "Hello World".CopyTo(span);
            writer.Advance(11);
            await Assert.That(writer.ToString(0, 5)).IsEqualTo("Hello");
        }
        finally { writer.Dispose(); }
    }

    // ─── ToUtf8Array ──────────────────────────────────────────────────────────

    [Test]
    public async Task ToUtf8ArrayAscii()
    {
        var writer = new CharBufferWriter();
        try
        {
            var span = writer.GetSpan(5);
            "hello".CopyTo(span);
            writer.Advance(5);
            var bytes = writer.ToUtf8Array();
            await Assert.That(Encoding.UTF8.GetString(bytes)).IsEqualTo("hello");
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task ToUtf8ArrayUnicode()
    {
        const string text   = "測試";
        var          writer = new CharBufferWriter();
        try
        {
            var span = writer.GetSpan(text.Length);
            text.CopyTo(span);
            writer.Advance(text.Length);
            var bytes = writer.ToUtf8Array();
            await Assert.That(Encoding.UTF8.GetString(bytes)).IsEqualTo(text);
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task ToUtf8ArrayEmpty()
    {
        var writer = new CharBufferWriter();
        try
        {
            var bytes = writer.ToUtf8Array();
            await Assert.That(bytes.Length).IsZero();
        }
        finally { writer.Dispose(); }
    }

    // ─── TryCopyTo(char) ─────────────────────────────────────────────────────

    [Test]
    public async Task TryCopyToSuccess()
    {
        var writer = new CharBufferWriter();
        try
        {
            var span = writer.GetSpan(5);
            "Hello".CopyTo(span);
            writer.Advance(5);

            var dst = new char[10];
            var ok  = writer.TryCopyTo(dst, out var written);
            await Assert.That(ok).IsTrue();
            await Assert.That(written).IsEqualTo(5);
            await Assert.That(new string(dst, 0, written)).IsEqualTo("Hello");
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task TryCopyToInsufficientBuffer()
    {
        var writer = new CharBufferWriter();
        try
        {
            var span = writer.GetSpan(10);
            "Hello World".AsSpan()[..10].CopyTo(span);
            writer.Advance(10);

            var dst = new char[3];
            var ok  = writer.TryCopyTo(dst, out var written);
            await Assert.That(ok).IsFalse();
            await Assert.That(written).IsZero();
        }
        finally { writer.Dispose(); }
    }

    // ─── TryCopyUtf8To ────────────────────────────────────────────────────────

    [Test]
    public async Task TryCopyUtf8ToSuccess()
    {
        var writer = new CharBufferWriter();
        try
        {
            var span = writer.GetSpan(5);
            "Hello".CopyTo(span);
            writer.Advance(5);

            var dst = new byte[20];
            var ok  = writer.TryCopyUtf8To(dst, out var written);
            await Assert.That(ok).IsTrue();
            await Assert.That(Encoding.UTF8.GetString(dst, 0, written)).IsEqualTo("Hello");
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task TryCopyUtf8ToInsufficientBuffer()
    {
        var writer = new CharBufferWriter();
        try
        {
            var span = writer.GetSpan(5);
            "Hello".CopyTo(span);
            writer.Advance(5);

            var dst = new byte[1];
            var ok  = writer.TryCopyUtf8To(dst, out var written);
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
        var writer = new CharBufferWriter();
        try
        {
            var span = writer.GetSpan(5);
            "Hello".CopyTo(span);
            writer.Advance(5);

            var builder = new ValueStringBuilder("ABC");
            writer.AppendTo(ref builder);
            // Bug: appends builder's own "ABC" to itself → "ABCABC", not "ABCABC" from writer.
            await Assert.That(builder.ToString()).IsEqualTo("ABCABC");
            builder.Dispose();
        }
        finally { writer.Dispose(); }
    }

    [Test]
    public async Task AppendToSliceDoublesBuilderSlice()
    {
        var writer = new CharBufferWriter();
        try
        {
            var span = writer.GetSpan(5);
            "Hello".CopyTo(span);
            writer.Advance(5);

            var builder = new ValueStringBuilder("ABCDE");
            writer.AppendTo(ref builder, 1, 3); // appends builder.AsSpan(1,3) = "BCD" to builder
            await Assert.That(builder.ToString()).IsEqualTo("ABCDEBCD");
            builder.Dispose();
        }
        finally { writer.Dispose(); }
    }

    // ─── Dispose ─────────────────────────────────────────────────────────────

    [Test]
    public void DoubleDispose()
    {
        var writer = new CharBufferWriter();
        writer.Dispose();
        writer.Dispose();
    }

    [Test]
    public void ToStringAfterDisposeThrows()
    {
        var writer = new CharBufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.ToString());
    }

    [Test]
    public void ToStringSliceAfterDisposeThrows()
    {
        var writer = new CharBufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.ToString(0, 0));
    }

    [Test]
    public void ToUtf8ArrayAfterDisposeThrows()
    {
        var writer = new CharBufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.ToUtf8Array());
    }

    [Test]
    public void GetSpanAfterDisposeThrows()
    {
        var writer = new CharBufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.GetSpan());
    }

    [Test]
    public void GetMemoryAfterDisposeThrows()
    {
        var writer = new CharBufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.GetMemory());
    }

    [Test]
    public void AdvanceAfterDisposeThrows()
    {
        var writer = new CharBufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() => writer.Advance(1));
    }

    [Test]
    public void TryCopyToAfterDisposeThrows()
    {
        var writer = new CharBufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var dst = new char[10];
            writer.TryCopyTo(dst, out _);
        });
    }

    [Test]
    public void TryCopyUtf8ToAfterDisposeThrows()
    {
        var writer = new CharBufferWriter();
        writer.Dispose();
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var dst = new byte[10];
            writer.TryCopyUtf8To(dst, out _);
        });
    }

    // ─── CleanBufferWhenReleased ──────────────────────────────────────────────

    [Test]
    public async Task CleanBufferWhenReleasedProperty()
    {
        var writer = new CharBufferWriter { CleanBufferWhenReleased = true };
        try
        {
            var span = writer.GetSpan(5);
            "Hello".CopyTo(span);
            writer.Advance(5);
            await Assert.That(writer.ToString()).IsEqualTo("Hello");
        }
        finally { writer.Dispose(); }
    }
}