using System.Text;

namespace Astra.Text.Tests;

public class AStringWriterTest
{
    // ─── Dispose ──────────────────────────────────────────────────────────────

    [Test]
    public void DoubleDisposeTest()
    {
        var writer = new AStringWriter();
        writer.Dispose();
        writer.Dispose(); // second call must not throw
    }

    [Test]
    public void CloseTest()
    {
        var writer = new AStringWriter();
        writer.Close();
        writer.Close(); // second call must not throw
    }

    // ─── Encoding ─────────────────────────────────────────────────────────────

    [Test]
    public async Task EncodingIsUnicode()
    {
        using var writer = new AStringWriter();
        await Assert.That(writer.Encoding).IsEqualTo(Encoding.Unicode);
    }

    // ─── Write(char) ─────────────────────────────────────────────────────────

    [Test]
    public async Task WriteChar()
    {
        using var writer = new AStringWriter();
        writer.Write('A');
        writer.Write('B');
        writer.Write('C');
        await Assert.That(writer.ToString()).IsEqualTo("ABC");
    }

    [Test]
    public async Task WriteCharUnicode()
    {
        using var writer = new AStringWriter();
        writer.Write('測');
        writer.Write('試');
        await Assert.That(writer.ToString()).IsEqualTo("測試");
    }

    // ─── Write(string?) ───────────────────────────────────────────────────────

    [Test]
    public async Task WriteString()
    {
        using var writer = new AStringWriter();
        writer.Write("Hello");
        writer.Write(" World");
        await Assert.That(writer.ToString()).IsEqualTo("Hello World");
    }

    [Test]
    public async Task WriteNullStringIsNoOp()
    {
        using var writer = new AStringWriter();
        writer.Write("before");
        writer.Write((string?)null);
        writer.Write("after");
        await Assert.That(writer.ToString()).IsEqualTo("beforeafter");
    }

    // ─── Write(char[], int, int) ──────────────────────────────────────────────

    [Test]
    public async Task WriteCharArray()
    {
        using var writer = new AStringWriter();
        writer.Write(new[] { 'H', 'e', 'l', 'l', 'o' }, 0, 5);
        await Assert.That(writer.ToString()).IsEqualTo("Hello");
    }

    [Test]
    public async Task WriteCharArrayWithOffset()
    {
        using var writer = new AStringWriter();
        writer.Write(new[] { 'H', 'e', 'l', 'l', 'o' }, 2, 3);
        await Assert.That(writer.ToString()).IsEqualTo("llo");
    }

    [Test]
    public void WriteNullCharArrayThrows()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            using var writer = new AStringWriter();
            writer.Write(null!, 0, 0);
        });
    }

    [Test]
    public void WriteCharArrayNegativeIndexThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var writer = new AStringWriter();
            writer.Write(new[] { 'A' }, -1, 1);
        });
    }

    [Test]
    public void WriteCharArrayNegativeCountThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var writer = new AStringWriter();
            writer.Write(new[] { 'A' }, 0, -1);
        });
    }

    [Test]
    public void WriteCharArrayCountExceedsLengthThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var writer = new AStringWriter();
            writer.Write(new[] { 'A', 'B' }, 0, 10);
        });
    }

    // ─── Write(bool) ──────────────────────────────────────────────────────────

    [Test]
    public async Task WriteBoolTrue()
    {
        using var writer = new AStringWriter();
        writer.Write(true);
        await Assert.That(writer.ToString()).IsEqualTo("True");
    }

    [Test]
    public async Task WriteBoolFalse()
    {
        using var writer = new AStringWriter();
        writer.Write(false);
        await Assert.That(writer.ToString()).IsEqualTo("False");
    }

    // ─── Write(decimal) ───────────────────────────────────────────────────────

    [Test]
    public async Task WriteDecimal()
    {
        using var writer = new AStringWriter();
        writer.Write(123.456m);
        var bcl = new StringBuilder();
        bcl.Append(123.456m);
        await Assert.That(writer.ToString()).IsEqualTo(bcl.ToString());
    }

    // ─── Write(ReadOnlySpan<char>) ────────────────────────────────────────────

    [Test]
    public async Task WriteReadOnlySpan()
    {
        using var writer = new AStringWriter();
        writer.Write("Hello".AsSpan());
        await Assert.That(writer.ToString()).IsEqualTo("Hello");
    }

    [Test]
    public async Task WriteReadOnlySpanEmpty()
    {
        using var writer = new AStringWriter();
        writer.Write(ReadOnlySpan<char>.Empty);
        await Assert.That(writer.ToString()).IsEqualTo(string.Empty);
    }

    // ─── WriteLine(ReadOnlySpan<char>) ────────────────────────────────────────

    [Test]
    public async Task WriteLineReadOnlySpan()
    {
        using var writer = new AStringWriter();
        writer.WriteLine("Hello".AsSpan());
        await Assert.That(writer.ToString()).IsEqualTo("Hello" + Environment.NewLine);
    }

    [Test]
    public async Task WriteLineEmpty()
    {
        using var writer = new AStringWriter();
        writer.WriteLine();
        await Assert.That(writer.ToString()).IsEqualTo(Environment.NewLine);
    }

    // ─── Async Write variants ─────────────────────────────────────────────────

    [Test]
    public async Task WriteAsyncChar()
    {
        using var writer = new AStringWriter();
        await writer.WriteAsync('X');
        await Assert.That(writer.ToString()).IsEqualTo("X");
    }

    [Test]
    public async Task WriteAsyncString()
    {
        using var writer = new AStringWriter();
        await writer.WriteAsync("async");
        await Assert.That(writer.ToString()).IsEqualTo("async");
    }

    [Test]
    public async Task WriteAsyncNullString()
    {
        using var writer = new AStringWriter();
        await writer.WriteAsync((string?)null);
        await Assert.That(writer.ToString()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task WriteAsyncCharArray()
    {
        using var writer = new AStringWriter();
        await writer.WriteAsync(new[] { 'H', 'i' }, 0, 2);
        await Assert.That(writer.ToString()).IsEqualTo("Hi");
    }

    [Test]
    public async Task WriteAsyncReadOnlyMemory()
    {
        using var writer = new AStringWriter();
        await writer.WriteAsync("Hello".AsMemory());
        await Assert.That(writer.ToString()).IsEqualTo("Hello");
    }

    [Test]
    public async Task WriteAsyncReadOnlyMemoryCancelled()
    {
        using var writer = new AStringWriter();
        var       token  = new CancellationToken(true);
        var       task   = writer.WriteAsync("Hello".AsMemory(), token);
        await Assert.That(task.IsCanceled).IsTrue();
    }

    // ─── Async WriteLine variants ─────────────────────────────────────────────

    [Test]
    public async Task WriteLineAsyncChar()
    {
        using var writer = new AStringWriter();
        await writer.WriteLineAsync('X');
        await Assert.That(writer.ToString()).IsEqualTo("X" + Environment.NewLine);
    }

    [Test]
    public async Task WriteLineAsyncString()
    {
        using var writer = new AStringWriter();
        await writer.WriteLineAsync("hello");
        await Assert.That(writer.ToString()).IsEqualTo("hello" + Environment.NewLine);
    }

    [Test]
    public async Task WriteLineAsyncNullString()
    {
        using var writer = new AStringWriter();
        await writer.WriteLineAsync((string?)null);
        await Assert.That(writer.ToString()).IsEqualTo(Environment.NewLine);
    }

    [Test]
    public async Task WriteLineAsyncCharArray()
    {
        using var writer = new AStringWriter();
        await writer.WriteLineAsync(new[] { 'H', 'i' }, 0, 2);
        await Assert.That(writer.ToString()).IsEqualTo("Hi" + Environment.NewLine);
    }

    [Test]
    public async Task WriteLineAsyncReadOnlyMemory()
    {
        using var writer = new AStringWriter();
        await writer.WriteLineAsync("Hello".AsMemory());
        await Assert.That(writer.ToString()).IsEqualTo("Hello" + Environment.NewLine);
    }

    [Test]
    public async Task WriteLineAsyncReadOnlyMemoryCancelled()
    {
        using var writer = new AStringWriter();
        var       token  = new CancellationToken(true);
        var       task   = writer.WriteLineAsync("Hello".AsMemory(), token);
        await Assert.That(task.IsCanceled).IsTrue();
    }

    // ─── FlushAsync ───────────────────────────────────────────────────────────

    [Test]
    public async Task FlushAsyncIsNoOp()
    {
        using var writer = new AStringWriter();
        writer.Write("data");
        await writer.FlushAsync();
        await Assert.That(writer.ToString()).IsEqualTo("data");
    }

    // ─── Mixed writes ─────────────────────────────────────────────────────────

    [Test]
    public async Task MixedWrites()
    {
        using var writer = new AStringWriter();
        writer.Write("text1".AsSpan());
        writer.Write("text2");
        writer.Write('c');
        writer.Write(true);
        writer.Write(123);
        writer.Write(456f);
        writer.Write(789d);
        writer.WriteLine();

        var expected = "text1text2cTrue123456789" + Environment.NewLine;
        await Assert.That(writer.ToString()).IsEqualTo(expected);
    }

    // ─── Disposed writer ──────────────────────────────────────────────────────

    [Test]
    public void WriteAfterDisposeThrows()
    {
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var writer = new AStringWriter();
            writer.Dispose();
            writer.Write('x');
        });
    }

    [Test]
    public void WriteStringAfterDisposeThrows()
    {
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var writer = new AStringWriter();
            writer.Dispose();
            writer.Write("hello");
        });
    }

    [Test]
    public void WriteCharArrayAfterDisposeThrows()
    {
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var writer = new AStringWriter();
            writer.Dispose();
            writer.Write(new[] { 'A' }, 0, 1);
        });
    }

    [Test]
    public void WriteBoolAfterDisposeThrows()
    {
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var writer = new AStringWriter();
            writer.Dispose();
            writer.Write(true);
        });
    }

    [Test]
    public void WriteDecimalAfterDisposeThrows()
    {
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var writer = new AStringWriter();
            writer.Dispose();
            writer.Write(1.0m);
        });
    }

    [Test]
    public void WriteSpanAfterDisposeThrows()
    {
        Assert.Throws<ObjectDisposedException>(() =>
        {
            var writer = new AStringWriter();
            writer.Dispose();
            writer.Write("x".AsSpan());
        });
    }
}