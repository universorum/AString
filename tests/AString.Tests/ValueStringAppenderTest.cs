using System.Buffers;
using System.Text;

namespace Astra.Text.Tests;

public class ValueStringAppenderTestCore
{
    [Test]
    public async Task Init()
    {
        var builder = new ValueStringAppender();
        await Assert.That(builder.Length).IsZero();
        await Assert.That(builder.MaxCapacity).IsEqualTo(int.MaxValue);
        await Assert.That(builder.Capacity).IsGreaterThan(0);
        await Assert.That(builder.Capacity).IsLessThan(int.MaxValue);
        await Assert.That(builder.ToString()).IsEquatableTo(string.Empty);
    }

    [Test]
    public void DisposeTest()
    {
        var sb = new ValueStringAppender();
        sb.Dispose();
        sb.Dispose(); // call more than once
    }

    [Test]
    public async Task Unicode()
    {
        var testString =
            "\u0030\u0031\u0032\u0033\u0054\u0065\u0073\u0074\u6e2c\u8a66\u1e87\u0353\u031e\u0352\u035f\u0361\u01eb\u0320\u0320\u0309\u030f\u0360\u0361\u0345\u0072\u032c\u033a\u035a\u030d\u035b\u0314\u0352\u0362\u0064\u0320\u034e\u0317\u0333\u0347\u0346\u030b\u030a\u0342\u0350\ud83d\udeb5\ud83c\udffb\u200d\u2640\ufe0f\u0022";

        using var a   = new ValueStringAppender(testString);
        var       bcl = new StringBuilder(testString);

        await Assert.That(a.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    [Arguments(int.MinValue, int.MinValue)]
    [Arguments(0,            -1)]
    [Arguments(-1,           1)]
    [Arguments(-12,          12)]
    [Arguments(-123,         123)]
    [Arguments(-1234,        1234)]
    [Arguments(-12345,       12345)]
    [Arguments(-123456,      123456)]
    [Arguments(-1234567,     1234567)]
    [Arguments(-12345678,    12345678)]
    [Arguments(-123456789,   123456789)]
    [Arguments(-1234567890,  1234567890)]
    public async Task Integer(int x, int y)
    {
        using var sb1 = new ValueStringAppender();
        using var sb3 = new ValueStringAppender();
        var       sb5 = new StringBuilder();
        sb1.Append(x);
        sb1.Append(y);
        sb3.Append(x);
        sb3.Append(y);
        sb5.Append(x);
        sb5.Append(y);

        await Assert.That(sb1.ToString()).IsEqualTo(sb3.ToString());
        await Assert.That(sb1.ToString()).IsEqualTo(sb5.ToString());
    }

    [Test]
    [Arguments(ulong.MinValue, ulong.MinValue)]
    [Arguments(0UL,            1UL)]
    [Arguments(1UL,            1UL)]
    [Arguments(12UL,           12UL)]
    [Arguments(123UL,          123UL)]
    [Arguments(1234UL,         1234UL)]
    [Arguments(12345UL,        12345UL)]
    [Arguments(123456UL,       123456UL)]
    [Arguments(1234567UL,      1234567UL)]
    [Arguments(12345678UL,     12345678UL)]
    [Arguments(123456789UL,    123456789UL)]
    [Arguments(1234567890UL,   1234567890UL)]
    [Arguments(12345678901UL,  12345678901UL)]
    [Arguments(123456789012UL, 123456789012UL)]
    public async Task UInt64(ulong x, ulong y)
    {
        using var sb1 = new ValueStringAppender();
        using var sb3 = new ValueStringAppender();
        var       sb5 = new StringBuilder();
        sb1.Append(x);
        sb1.Append(y);
        sb3.Append(x);
        sb3.Append(y);
        sb5.Append(x);
        sb5.Append(y);

        await Assert.That(sb1.ToString()).IsEqualTo(sb3.ToString());
        await Assert.That(sb1.ToString()).IsEqualTo(sb5.ToString());
    }

    [Test]
    [Arguments(0.1,         -0.1)]
    [Arguments(0.0,         0.0)]
    [Arguments(0.12,        0.12)]
    [Arguments(0.123,       0.123)]
    [Arguments(0.1234,      0.1234)]
    [Arguments(0.12345,     0.12345)]
    [Arguments(1.12345,     1.12345)]
    [Arguments(12.12345,    12.12345)]
    [Arguments(123.12345,   123.12345)]
    [Arguments(1234.12345,  1234.12345)]
    [Arguments(12345.12345, 12345.12345)]
    [Arguments(1234512345d, 1234512345d)]
    public async Task Double(double x, double y)
    {
        using var sb1 = new ValueStringAppender();
        using var sb3 = new ValueStringAppender();
        var       sb5 = new StringBuilder();
        sb1.Append(x);
        sb1.Append(y);
        sb3.Append(x);
        sb3.Append(y);
        sb5.Append(x);
        sb5.Append(y);

        await Assert.That(sb1.ToString()).IsEqualTo(sb3.ToString());
        await Assert.That(sb1.ToString()).IsEqualTo(sb5.ToString());
    }

    [Test]
    //[Arguments(double.MinValue, double.MinValue)]
    //[Arguments(double.Epsilon, double.NaN)]
    [Arguments(0.1f,      -0.1f)]
    [Arguments(0.0f,      0.0f)]
    [Arguments(0.12f,     0.12f)]
    [Arguments(0.123f,    0.123f)]
    [Arguments(0.1234f,   0.1234f)]
    [Arguments(0.12345f,  0.12345f)]
    [Arguments(1.12345f,  1.12345f)]
    [Arguments(12.12345f, 12.12345f)]
    [Arguments(123.123f,  123.123f)]
    public async Task Single(float x, float y)
    {
        using var sb1 = new ValueStringAppender();
        using var sb3 = new ValueStringAppender();
        var       sb5 = new StringBuilder();
        sb1.Append(x);
        sb1.Append(y);
        sb3.Append(x);
        sb3.Append(y);
        sb5.Append(x);
        sb5.Append(y);

        await Assert.That(sb1.ToString()).IsEqualTo(sb3.ToString());
        await Assert.That(sb1.ToString()).IsEqualTo(sb5.ToString());
    }

    [Test]
    public async Task Others()
    {
        using var sb1 = new ValueStringAppender();
        using var sb3 = new ValueStringAppender();
        var       x   = DateTime.Now;
        var       y   = DateTimeOffset.Now;
        var       z   = TimeSpan.FromMilliseconds(12345.6789);
        var       g   = Guid.NewGuid();

        var sb5 = new StringBuilder();
        sb1.Append(x);
        sb1.Append(y);
        sb1.Append(z);
        sb1.Append(g);
        sb3.Append(x);
        sb3.Append(y);
        sb3.Append(z);
        sb3.Append(g);
        sb5.Append(x);
        sb5.Append(y);
        sb5.Append(z);
        sb5.Append(g);

        await Assert.That(sb1.ToString()).IsEqualTo(sb3.ToString());
        await Assert.That(sb1.ToString()).IsEqualTo(sb5.ToString());
    }

    [Test]
    public async Task EnumTest()
    {
        var x = MoreMyEnum.Apple;
        var y = MoreMyEnum.Orange;

        using var sb1 = new ValueStringAppender();
        using var sb3 = new ValueStringAppender();
        var       sb5 = new StringBuilder();
        sb1.Append(x);
        sb1.Append(y);
        sb3.Append(x);
        sb3.Append(y);
        sb5.Append(x);
        sb5.Append(y);

        await Assert.That(sb1.ToString()).IsEqualTo(sb3.ToString());
        await Assert.That(sb1.ToString()).IsEqualTo(sb5.ToString());
    }

    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public async Task BoolTest(bool x)
    {
        using var sb1 = new ValueStringAppender();
        using var sb3 = new ValueStringAppender();
        var       sb5 = new StringBuilder();
        sb1.Append(x);
        sb3.Append(x);
        sb5.Append(x);

        await Assert.That(sb1.ToString()).IsEqualTo(sb3.ToString());
        await Assert.That(sb1.ToString()).IsEqualTo(sb5.ToString());
    }

    // ─── Constructors ──────────────────────────────────────────────────────────

    [Test]
    public async Task ConstructorWithSizeHint()
    {
        using var sb = new ValueStringAppender(64);
        await Assert.That(sb.Length).IsZero();
        await Assert.That(sb.Capacity).IsGreaterThan(0);
        await Assert.That(sb.MaxCapacity).IsEqualTo(int.MaxValue);
    }

    [Test]
    public async Task ConstructorWithSizeHintAndMaxCapacity()
    {
        using var sb = new ValueStringAppender(64, 1000);
        await Assert.That(sb.MaxCapacity).IsEqualTo(1000);
    }

    [Test]
    public async Task ConstructorWithString()
    {
        using var sb = new ValueStringAppender("hello");
        await Assert.That(sb.ToString()).IsEqualTo("hello");
        await Assert.That(sb.Length).IsEqualTo(5);
    }

    [Test]
    public async Task ConstructorWithNullString()
    {
        using var sb = new ValueStringAppender(null);
        await Assert.That(sb.Length).IsZero();
        await Assert.That(sb.ToString()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task ConstructorWithStringStartIndexAndLength()
    {
        using var sb = new ValueStringAppender("hello world", 6, 5, 64);
        await Assert.That(sb.ToString()).IsEqualTo("world");
    }

    [Test]
    public async Task ConstructorWithNullStringStartIndexAndLength()
    {
        using var sb = new ValueStringAppender(null, 0, 0, 64);
        await Assert.That(sb.Length).IsZero();
    }

    // ─── Equals ────────────────────────────────────────────────────────────────

    [Test]
    public async Task EqualsReadOnlySpanTrue()
    {
        using var sb = new ValueStringAppender("hello");
        await Assert.That(sb.Equals("hello".AsSpan())).IsTrue();
    }

    [Test]
    public async Task EqualsReadOnlySpanFalse()
    {
        using var sb = new ValueStringAppender("hello");
        await Assert.That(sb.Equals("world".AsSpan())).IsFalse();
        await Assert.That(sb.Equals("hell".AsSpan())).IsFalse();
        await Assert.That(sb.Equals("helloo".AsSpan())).IsFalse();
    }

    [Test]
    public async Task EqualsReadOnlySpanEmpty()
    {
        using var sb = new ValueStringAppender();
        await Assert.That(sb.Equals(ReadOnlySpan<char>.Empty)).IsTrue();
        await Assert.That(sb.Equals("x".AsSpan())).IsFalse();
    }

    [Test]
    public async Task EqualsValueStringAppenderTrue()
    {
        using var sb1 = new ValueStringAppender("hello");
        using var sb2 = new ValueStringAppender("hello");
        await Assert.That(sb1.Equals(sb2)).IsTrue();
    }

    [Test]
    public async Task EqualsValueStringAppenderFalse()
    {
        using var sb1 = new ValueStringAppender("hello");
        using var sb2 = new ValueStringAppender("world");
        using var sb3 = new ValueStringAppender("hell");
        await Assert.That(sb1.Equals(sb2)).IsFalse();
        await Assert.That(sb1.Equals(sb3)).IsFalse();
    }

    [Test]
    public async Task EqualsStringBuilderTrue()
    {
        using var sb1 = new ValueStringAppender("hello world");
        var       sb2 = new StringBuilder("hello world");
        await Assert.That(sb1.Equals(sb2)).IsTrue();
    }

    [Test]
    public async Task EqualsStringBuilderFalse()
    {
        using var sb1 = new ValueStringAppender("hello");
        var       sb2 = new StringBuilder("different");
        var       sb3 = new StringBuilder("hell");
        await Assert.That(sb1.Equals(sb2)).IsFalse();
        await Assert.That(sb1.Equals(sb3)).IsFalse();
    }

    [Test]
    public async Task EqualsStringBuilderDifferentLength()
    {
        using var sb1 = new ValueStringAppender("hi");
        var       sb2 = new StringBuilder("hello");
        await Assert.That(sb1.Equals(sb2)).IsFalse();
    }

    // ─── ToString overloads ────────────────────────────────────────────────────

    [Test]
    public async Task ToStringSliceMid()
    {
        using var sb = new ValueStringAppender("hello world");
        await Assert.That(sb.ToString(6, 5)).IsEqualTo("world");
    }

    [Test]
    public async Task ToStringSliceStart()
    {
        using var sb = new ValueStringAppender("hello world");
        await Assert.That(sb.ToString(0, 5)).IsEqualTo("hello");
    }

    [Test]
    public async Task ToStringSliceEmpty()
    {
        using var sb = new ValueStringAppender("hello");
        await Assert.That(sb.ToString(0, 0)).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task ToStringSliceFull()
    {
        using var sb = new ValueStringAppender("hello world");
        await Assert.That(sb.ToString(0, 11)).IsEqualTo("hello world");
    }

    [Test]
    public async Task ToStringLargeMid()
    {
        var random = new Random(1337);

        var str = string.Create(65536,
            random,
            static (span, random) =>
            {
                while (!span.IsEmpty)
                {
                    var num = random.Next(int.MinValue, int.MaxValue);
                    if (!num.TryFormat(span, out var written)) { break; }

                    span = span[written..];
                }

                span.Fill('x');
            });


        using var sb = new ValueStringAppender(str);
        await Assert.That(sb.ToString(1, 65534)).IsEqualTo(str[1..^1]);
    }

    [Test]
    public async Task ToStringLarge()
    {
        var random = new Random(1337);

        var str = string.Create(65536,
            random,
            static (span, random) =>
            {
                while (!span.IsEmpty)
                {
                    var num = random.Next(int.MinValue, int.MaxValue);
                    if (!num.TryFormat(span, out var written)) { break; }

                    span = span[written..];
                }

                span.Fill('x');
            });


        using var sb = new ValueStringAppender(str);
        await Assert.That(sb.ToString()).IsEqualTo(str);
    }

    // ─── CopyTo ────────────────────────────────────────────────────────────────

    [Test]
    public async Task CopyToSpanFromStart()
    {
        using var sb  = new ValueStringAppender("hello");
        var       dst = new char[5];
        sb.CopyTo(0, dst, 0, 5);
        await Assert.That(new string(dst)).IsEqualTo("hello");
    }

    [Test]
    public async Task CopyToSpanFromOffset()
    {
        using var sb  = new ValueStringAppender("hello world");
        var       dst = new char[5];
        sb.CopyTo(6, dst, 0, 5);
        await Assert.That(new string(dst)).IsEqualTo("world");
    }

    [Test]
    public async Task CopyToCharArray()
    {
        using var sb  = new ValueStringAppender("hello");
        var       dst = new char[5];
        sb.CopyTo(0, dst, 0, 5);
        await Assert.That(new string(dst)).IsEqualTo("hello");
    }

    [Test]
    public async Task CopyToCharArrayWithSourceOffset()
    {
        using var sb  = new ValueStringAppender("hello world");
        var       dst = new char[5];
        sb.CopyTo(6, dst, 0, 5);
        await Assert.That(new string(dst)).IsEqualTo("world");
    }

    [Test]
    public async Task CopyToCharArrayWithDestOffset()
    {
        using var sb  = new ValueStringAppender("hello");
        var       dst = new char[10];
        sb.CopyTo(0, dst, 3, 5);
        await Assert.That(new string(dst, 3, 5)).IsEqualTo("hello");
    }

    // ─── TryCopyTo ────────────────────────────────────────────────────────────

    [Test]
    public async Task TryCopyToSuccess()
    {
        using var sb      = new ValueStringAppender("hello");
        var       dst     = new char[10];
        var       success = sb.TryCopyTo(dst, out var written);
        await Assert.That(success).IsTrue();
        await Assert.That(written).IsEqualTo(5);
        await Assert.That(new string(dst, 0, written)).IsEqualTo("hello");
    }

    [Test]
    public async Task TryCopyToExactSize()
    {
        using var sb      = new ValueStringAppender("hello");
        var       dst     = new char[5];
        var       success = sb.TryCopyTo(dst, out var written);
        await Assert.That(success).IsTrue();
        await Assert.That(written).IsEqualTo(5);
    }

    [Test]
    public async Task TryCopyToInsufficientSpace()
    {
        using var sb      = new ValueStringAppender("hello world");
        var       dst     = new char[3];
        var       success = sb.TryCopyTo(dst, out var written);
        await Assert.That(success).IsFalse();
        await Assert.That(written).IsZero();
    }

    // ─── EnsureCapacity ───────────────────────────────────────────────────────

    [Test]
    public async Task EnsureCapacityGrowsBuffer()
    {
        using var sb  = new ValueStringAppender(16);
        var       cap = sb.EnsureCapacity(1000);
        await Assert.That(cap).IsGreaterThan(999);
        await Assert.That(sb.Capacity).IsGreaterThan(999);
    }

    [Test]
    public async Task EnsureCapacityNoOpWhenLarger()
    {
        using var sb     = new ValueStringAppender(1024);
        var       oldCap = sb.Capacity;
        var       newCap = sb.EnsureCapacity(1);
        await Assert.That(newCap).IsEqualTo(oldCap);
    }

    [Test]
    public async Task EnsureCapacityPreservesContent()
    {
        using var sb = new ValueStringAppender("hello");
        sb.EnsureCapacity(5000);
        await Assert.That(sb.ToString()).IsEqualTo("hello");
    }

    // ─── Clear ────────────────────────────────────────────────────────────────

    [Test]
    public async Task ClearResetsLength()
    {
        using var sb = new ValueStringAppender("hello world");
        sb.Clear();
        await Assert.That(sb.Length).IsZero();
        await Assert.That(sb.ToString()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task ClearThenAppend()
    {
        using var sb = new ValueStringAppender("old content");
        sb.Clear();
        sb.Append("new");
        await Assert.That(sb.ToString()).IsEqualTo("new");
    }

    // ─── Length setter ────────────────────────────────────────────────────────
    // Note: 'using var' makes struct variable readonly for setters; use var + explicit Dispose.

    [Test]
    public async Task LengthSetterTruncates()
    {
        var sb = new ValueStringAppender("hello world");
        try
        {
            sb.Length = 5;
            await Assert.That(sb.ToString()).IsEqualTo("hello");
        }
        finally { sb.Dispose(); }
    }

    [Test]
    public async Task LengthSetterZero()
    {
        var sb = new ValueStringAppender("hello");
        try
        {
            sb.Length = 0;
            await Assert.That(sb.ToString()).IsEqualTo(string.Empty);
        }
        finally { sb.Dispose(); }
    }

    [Test]
    public void LengthSetterNegativeThrows()
    {
        var sb = new ValueStringAppender("hello");
        try { Assert.Throws<ArgumentOutOfRangeException>(() => sb.Length = -1); }
        finally { sb.Dispose(); }
    }

    // ─── Capacity setter ─────────────────────────────────────────────────────

    [Test]
    public async Task CapacitySetterGrows()
    {
        var sb = new ValueStringAppender(16);
        try
        {
            sb.Capacity = 2000;
            await Assert.That(sb.Capacity).IsGreaterThan(1999);
        }
        finally { sb.Dispose(); }
    }

    [Test]
    public async Task CapacitySetterNoOpWhenSmaller()
    {
        var sb = new ValueStringAppender(1024);
        try
        {
            var oldCap = sb.Capacity;
            sb.Capacity = 1;
            await Assert.That(sb.Capacity).IsEqualTo(oldCap);
        }
        finally { sb.Dispose(); }
    }

    // ─── Indexer ──────────────────────────────────────────────────────────────

    [Test]
    public async Task IndexerGetFirst()
    {
        using var sb = new ValueStringAppender("hello");
        await Assert.That(sb[0]).IsEqualTo('h');
    }

    [Test]
    public async Task IndexerGetLast()
    {
        using var sb = new ValueStringAppender("hello");
        await Assert.That(sb[4]).IsEqualTo('o');
    }

    [Test]
    public async Task IndexerSet()
    {
        var sb = new ValueStringAppender("hello");
        try
        {
            sb[0] = 'H';
            await Assert.That(sb.ToString()).IsEqualTo("Hello");
        }
        finally { sb.Dispose(); }
    }

    [Test]
    public async Task IndexerSetMid()
    {
        var sb = new ValueStringAppender("hello");
        try
        {
            sb[2] = 'L';
            await Assert.That(sb.ToString()).IsEqualTo("heLlo");
        }
        finally { sb.Dispose(); }
    }

    [Test]
    public void IndexerGetOutOfRangeThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var sb = new ValueStringAppender("hello");
            _ = sb[10];
        });
    }

    [Test]
    public void IndexerSetOutOfRangeThrows()
    {
        var sb = new ValueStringAppender("hello");
        try { Assert.Throws<ArgumentOutOfRangeException>(() => sb[10] = 'x'); }
        finally { sb.Dispose(); }
    }

    [Test]
    public void IndexerNegativeThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            using var sb = new ValueStringAppender("hello");
            _ = sb[-1];
        });
    }

    private enum MoreMyEnum { Fruit, Apple, Orange }
}