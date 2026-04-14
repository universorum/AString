using System.Buffers;
using System.Text;

namespace Astra.Text.Tests;

public class ValueStringBuilderTestAppend
{
    // ─── Append(string?) ─────────────────────────────────────────

    [Test]
    [Arguments("str")]
    [Arguments("")]
    [Arguments("null")]
    public async Task AppendString(string? value)
    {
        using var sb1 = new ValueStringBuilder();
        sb1.Append(value);
        await Assert.That(sb1.ToString()).IsEqualTo(value);
    }

    // ─── Append(char[]) ───────────────────────────────────────────────────────

    [Test]
    public async Task AppendCharArray()
    {
        using var sb  = new ValueStringBuilder();
        var       bcl = new StringBuilder();
        var       arr = "hello".ToCharArray();
        sb.Append(arr);
        bcl.Append(arr);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendNullCharArray()
    {
        using var sb = new ValueStringBuilder();
        sb.Append((char[]?)null);
        await Assert.That(sb.ToString()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task AppendEmptyCharArray()
    {
        using var sb = new ValueStringBuilder();
        sb.Append(Array.Empty<char>());
        await Assert.That(sb.ToString()).IsEqualTo(string.Empty);
    }

    // ─── Append(string?, int, int) ────────────────────────────────────────────

    [Test]
    [Arguments(0,  0)]
    [Arguments(6,  5)]
    [Arguments(0,  5)]
    [Arguments(0,  11)]
    [Arguments(10, 1)]
    [Arguments(11, 0)]
    public async Task AppendStringWithValidRange(int start, int count)
    {
        using var sb  = new ValueStringBuilder();
        var       bcl = new StringBuilder();
        sb.Append("hello world", start, count);
        bcl.Append("hello world", start, count);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendNullStringWithValidRange()
    {
        using var sb = new ValueStringBuilder();
        sb.Append((string?)null, 0, 0);
        await Assert.That(sb.ToString()).IsEqualTo(string.Empty);
    }

    [Test]
    [Arguments(12, 0)]
    [Arguments(10, 2)]
    public void AppendStringWithInvalidRange(int start, int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var sb = new ValueStringBuilder();
            sb.Append("hello world", start, count);
        });
    }

    [Test]
    public void AppendNullStringWithInvalidRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var sb = new ValueStringBuilder();
            sb.Append((string?)null, 1, 2);
        });
    }

    // ─── Append(char[], int, int) ─────────────────────────────────────────────

    [Test]
    [Arguments(6,  5)]
    [Arguments(0,  5)]
    [Arguments(0,  11)]
    [Arguments(10, 1)]
    [Arguments(11, 0)]
    public async Task AppendCharArrayWithValidRange(int start, int count)
    {
        using var sb  = new ValueStringBuilder();
        var       bcl = new StringBuilder();
        var       arr = "hello world".ToCharArray();
        sb.Append(arr, start, count);
        bcl.Append(arr, start, count);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendNullCharArrayWithValidRange()
    {
        using var sb = new ValueStringBuilder();
        sb.Append((char[]?)null, 0, 0);
        await Assert.That(sb.ToString()).IsEqualTo(string.Empty);
    }

    [Test]
    [Arguments(11, 1)]
    [Arguments(12, 0)]
    [Arguments(10, 2)]
    public void AppendCharArrayWithInvalidRange(int start, int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var       arr = "hello world".ToCharArray();
            using var sb  = new ValueStringBuilder();
            sb.Append(arr, start, count);
        });
    }

    [Test]
    public void AppendNullCharArrayWithInvalidRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var sb = new ValueStringBuilder();
            sb.Append((string?)null, 1, 2);
        });
    }

    // ─── Append(char, int) ────────────────────────────────────────────────────

    [Test]
    [Arguments('\0',     5)]
    [Arguments('0',      5)]
    [Arguments('\u6e2c', 5)]
    [Arguments('\0',     0)]
    [Arguments('0',      0)]
    [Arguments('\u6e2c', 0)]
    [Arguments('\0',     2000)]
    [Arguments('0',      2000)]
    [Arguments('\u6e2c', 2000)]
    public async Task AppendCharWithRepeat(char value, int repeat)
    {
        using var sb  = new ValueStringBuilder();
        var       bcl = new StringBuilder();
        sb.Append(value, repeat);
        bcl.Append(value, repeat);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    [Arguments('\0')]
    [Arguments('0')]
    [Arguments('\u6e2c')]
    public void AppendCharWithNegativeRepeat(char value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var sb = new ValueStringBuilder();
            sb.Append(value, -1);
        });
    }

    // ─── Append(Memory<char>) ────────────────────────────────────────────────────

    [Test]
    [Arguments("str")]
    [Arguments("")]
    [Arguments("null")]
    public async Task AppendMemory(string? value)
    {
        var ros   = value.AsSpan();
        var array = ArrayPool<char>.Shared.Rent(ros.Length);
        try
        {
            var memory = array.AsMemory(0, ros.Length);
            ros.CopyTo(array);

            using var sb1 = new ValueStringBuilder();
            sb1.Append(memory);
            await Assert.That(sb1.ToString()).IsEqualTo(value);
        }
        finally { ArrayPool<char>.Shared.Return(array); }
    }

    [Test]
    public async Task AppendMemoryEmpty()
    {
        using var sb  = new ValueStringBuilder("hi");
        var       mem = Memory<char>.Empty;
        sb.Append(mem);
        await Assert.That(sb.ToString()).IsEqualTo("hi");
    }

    // ─── Append(ReadOnlyMemory<char>) ────────────────────────────────────────────────────

    [Test]
    [Arguments("str")]
    [Arguments("")]
    [Arguments("null")]
    public async Task AppendReadOnlyMemory(string? value)
    {
        using var sb1 = new ValueStringBuilder();
        sb1.Append(value.AsMemory());
        await Assert.That(sb1.ToString()).IsEqualTo(value);
    }

    [Test]
    public async Task AppendReadOnlyMemoryEmpty()
    {
        using var sb  = new ValueStringBuilder("hi");
        var       mem = ReadOnlyMemory<char>.Empty;
        sb.Append(mem);
        await Assert.That(sb.ToString()).IsEqualTo("hi");
    }

    // ─── Append(Span<char>) ────────────────────────────────────────────────────

    [Test]
    [Arguments("str")]
    [Arguments("")]
    [Arguments("null")]
    public async Task AppendSpan(string? value)
    {
        var        ros  = value.AsSpan();
        Span<char> span = [..ros];
        using var  sb1  = new ValueStringBuilder();
        sb1.Append(span);
        await Assert.That(sb1.ToString()).IsEqualTo(value);
    }

    [Test]
    public async Task AppendSpanEmpty()
    {
        using var sb  = new ValueStringBuilder("hi");
        var       mem = Span<char>.Empty;
        sb.Append(mem);
        await Assert.That(sb.ToString()).IsEqualTo("hi");
    }

    // ─── Append(ReadOnlyMemory<char>) ────────────────────────────────────────────────────

    [Test]
    [Arguments("str")]
    [Arguments("")]
    [Arguments("null")]
    public async Task AppendReadOnlySpan(string? value)
    {
        using var sb1 = new ValueStringBuilder();
        sb1.Append(value.AsSpan());
        await Assert.That(sb1.ToString()).IsEqualTo(value);
    }

    [Test]
    public async Task AppendReadOnlySpanEmpty()
    {
        using var sb  = new ValueStringBuilder("hi");
        var       mem = ReadOnlySpan<char>.Empty;
        sb.Append(mem);
        await Assert.That(sb.ToString()).IsEqualTo("hi");
    }

    // ─── Append(ValueStringBuilder) ─────────────────────────────────────────

    [Test]
    public async Task AppendValueStringBuilder()
    {
        using var sb1 = new ValueStringBuilder("hello");
        using var sb2 = new ValueStringBuilder();
        sb2.Append(sb1);
        await Assert.That(sb2.ToString()).IsEqualTo("hello");
    }

    [Test]
    public async Task AppendValueStringBuilderToExisting()
    {
        using var sb1 = new ValueStringBuilder("world");
        using var sb2 = new ValueStringBuilder("hello ");
        sb2.Append(sb1);
        await Assert.That(sb2.ToString()).IsEqualTo("hello world");
    }

    [Test]
    public async Task AppendEmptyValueStringBuilder()
    {
        using var sb1 = new ValueStringBuilder();
        using var sb2 = new ValueStringBuilder("hello");
        sb2.Append(sb1);
        await Assert.That(sb2.ToString()).IsEqualTo("hello");
    }

    // ─── Append(Rune) ─────────────────────────────────────────

#if NETCOREAPP3_0_OR_GREATER
    [Test]
    [Arguments('0',      null)]
    [Arguments('\u6e2c', null)]
    [Arguments('\u00c5', null)]
    [Arguments('\ud83d', '\udd2e')]
    public async Task AppendRune(char char1, char? char2)
    {
        var str  = char2.HasValue ? new string([char1, char2.Value]) : new string([char1]);
        var rune = char2.HasValue ? new Rune(char1, char2.Value) : new Rune(char1);

        using var a = new ValueStringBuilder();
        a.Append(rune);

        await Assert.That(a.ToString()).IsEqualTo(str);
    }
#endif

    // ─── Chained appends ──────────────────────────────────────────────────────

    [Test]
    public async Task AppendVariousTypesChained()
    {
        using var sb  = new ValueStringBuilder();
        var       bcl = new StringBuilder();

        sb.Append("hello".ToCharArray());
        sb.Append(' ');
        sb.Append('w', 1);
        sb.Append("orld".AsMemory());

        bcl.Append("hello".ToCharArray());
        bcl.Append(' ');
        bcl.Append('w', 1);
        bcl.Append("orld");

        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }
}