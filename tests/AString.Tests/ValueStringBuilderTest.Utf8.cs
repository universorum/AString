using System.Text;

namespace Astra.Text.Tests;

public class ValueStringBuilderTestUtf8
{
    [Test]
    public async Task AppendCharRepeat()
    {
        using var zsb = new ValueStringBuilder();
        var       bcl = new StringBuilder();
        zsb.AppendUft8("foo"u8);
        bcl.Append("foo");
        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());

        zsb.Append('\x7F', 10);
        bcl.Append('\x7F', 10);
        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());

        zsb.Append('\x80', 10);
        bcl.Append('\x80', 10);
        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());

        zsb.Append('\u9bd6', 10);
        bcl.Append('\u9bd6', 10);
        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());

        zsb.AppendUft8("bar"u8);
        bcl.Append("bar");
        await Assert.That(zsb.ToString()).IsEqualTo(bcl.ToString());
    }

    // ─── ToUtf8 ───────────────────────────────────────────────────────────────

    [Test]
    public async Task ToUtf8AsciiRoundTrip()
    {
        using var sb    = new ValueStringBuilder("hello");
        var       bytes = sb.ToUtf8Array();
        await Assert.That(Encoding.UTF8.GetString(bytes)).IsEqualTo("hello");
    }

    [Test]
    public async Task ToUtf8UnicodeRoundTrip()
    {
        const string text  = "測試";
        using var    sb    = new ValueStringBuilder(text);
        var          bytes = sb.ToUtf8Array();
        await Assert.That(Encoding.UTF8.GetString(bytes)).IsEqualTo(text);
    }

    [Test]
    public async Task ToUtf8Empty()
    {
        using var sb    = new ValueStringBuilder();
        var       bytes = sb.ToUtf8Array();
        await Assert.That(bytes.Length).IsZero();
    }

    // ─── AppendUft8Line ───────────────────────────────────────────────────────

    [Test]
    public async Task AppendUft8Line()
    {
        using var sb  = new ValueStringBuilder();
        var       bcl = new StringBuilder();
        sb.AppendUft8Line("hello"u8);
        bcl.AppendLine("hello");
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendUft8LineEmpty()
    {
        using var sb  = new ValueStringBuilder();
        var       bcl = new StringBuilder();
        sb.AppendUft8Line(""u8);
        bcl.AppendLine();
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    // ─── GetUft8ByteCount ────────────────────────────────────────────────────

    [Test]
    public async Task GetUft8ByteCountAscii()
    {
        using var sb = new ValueStringBuilder("hello");
        await Assert.That(sb.GetUft8ByteCount()).IsEqualTo(Encoding.UTF8.GetByteCount("hello"));
    }

    [Test]
    public async Task GetUft8ByteCountUnicode()
    {
        const string text = "測試";
        using var    sb   = new ValueStringBuilder(text);
        await Assert.That(sb.GetUft8ByteCount()).IsEqualTo(Encoding.UTF8.GetByteCount(text));
    }

    [Test]
    public async Task GetUft8ByteCountEmpty()
    {
        using var sb = new ValueStringBuilder();
        await Assert.That(sb.GetUft8ByteCount()).IsZero();
    }

    // ─── GetUft8Bytes ─────────────────────────────────────────────────────────

    [Test]
    public async Task GetUft8BytesAscii()
    {
        using var sb     = new ValueStringBuilder("hello");
        var       buffer = new byte[sb.GetUft8ByteCount()];
        var       count  = sb.GetUft8Bytes(buffer);
        await Assert.That(count).IsEqualTo(5);
        await Assert.That(Encoding.UTF8.GetString(buffer, 0, count)).IsEqualTo("hello");
    }

    [Test]
    public async Task GetUft8BytesUnicode()
    {
        const string text   = "測試";
        using var    sb     = new ValueStringBuilder(text);
        var          buffer = new byte[sb.GetUft8ByteCount()];
        var          count  = sb.GetUft8Bytes(buffer);
        await Assert.That(Encoding.UTF8.GetString(buffer, 0, count)).IsEqualTo(text);
    }

    // ─── TryCopyUtf8To ────────────────────────────────────────────────────────
    // Use byte[] (implicitly converts to Span<byte>) to avoid stackalloc-across-await issues.

    [Test]
    public async Task TryCopyUtf8ToSuccess()
    {
        using var sb      = new ValueStringBuilder("hello");
        var       buffer  = new byte[20];
        var       success = sb.TryCopyUtf8To(buffer, out var written);
        await Assert.That(success).IsTrue();
        await Assert.That(Encoding.UTF8.GetString(buffer, 0, written)).IsEqualTo("hello");
    }

    [Test]
    public async Task TryCopyUtf8ToInsufficientBuffer()
    {
        using var sb      = new ValueStringBuilder("hello world");
        var       buffer  = new byte[2];
        var       success = sb.TryCopyUtf8To(buffer, out var written);
        await Assert.That(success).IsFalse();
        await Assert.That(written).IsZero();
    }

    // ─── CopyUtf8To(Span<byte>) ───────────────────────────────────────────────

    [Test]
    public async Task CopyUtf8ToSpan()
    {
        using var sb     = new ValueStringBuilder("hello");
        var       buffer = new byte[20];
        sb.CopyUtf8To(0, (Span<byte>)buffer, 5);
        await Assert.That(Encoding.UTF8.GetString(buffer, 0, 5)).IsEqualTo("hello");
    }

    // ─── CopyUtf8To(byte[]) ───────────────────────────────────────────────────

    [Test]
    public async Task CopyUtf8ToArray()
    {
        using var sb     = new ValueStringBuilder("hello");
        var       buffer = new byte[20];
        sb.CopyUtf8To(0, buffer, 0, 5);
        await Assert.That(Encoding.UTF8.GetString(buffer, 0, 5)).IsEqualTo("hello");
    }

    // ─── WriteToAsync ─────────────────────────────────────────────────────────

    [Test]
    public async Task WriteToAsyncWritesUtf8Bytes()
    {
        using var sb     = new ValueStringBuilder("hello");
        using var stream = new MemoryStream();
        await sb.WriteToAsync(stream);
        var bytes = stream.ToArray();
        await Assert.That(Encoding.UTF8.GetString(bytes)).IsEqualTo("hello");
    }

    [Test]
    public async Task WriteToAsyncUnicode()
    {
        const string text   = "測試";
        using var    sb     = new ValueStringBuilder(text);
        using var    stream = new MemoryStream();
        await sb.WriteToAsync(stream);
        var bytes = stream.ToArray();
        await Assert.That(Encoding.UTF8.GetString(bytes)).IsEqualTo(text);
    }

    [Test]
    public async Task WriteToAsyncEmpty()
    {
        using var sb     = new ValueStringBuilder();
        using var stream = new MemoryStream();
        await sb.WriteToAsync(stream);
        await Assert.That(stream.Length).IsZero();
    }
}