using System.Text;

namespace Astra.Text.Tests;

public class ValueStringAppenderTestAppend
{
    [Test]
    public async Task Format()
    {
        var bcl = new StringBuilder();
        var a   = new ValueStringAppender();

        var str = "String";
        bcl.Append(str);
        a.Append(str);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var boolean = true;
        bcl.Append(boolean);
        a.Append(boolean);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var int8 = (sbyte)2;
        bcl.Append(int8);
        a.Append(int8);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var int16 = (short)22;
        bcl.Append(int16);
        a.Append(int16);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var int32 = 1234567;
        bcl.Append(int32);
        a.Append(int32);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var int64 = (long)int.MaxValue + 123456;
        bcl.Append(int64);
        a.Append(int64);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var b = (byte)0x2;
        bcl.Append(b);
        a.Append(b);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var uint16 = (ushort)22;
        bcl.Append(uint16);
        a.Append(uint16);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var uint32 = (uint)1234567;
        bcl.Append(uint32);
        a.Append(uint32);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var uint64 = (ulong)int.MaxValue + 123456;
        bcl.Append(uint64);
        a.Append(uint64);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var single = (float)1.1;
        bcl.Append(single);
        a.Append(single);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var doubl = 1.1;
        bcl.Append(doubl);
        a.Append(doubl);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var ts = TimeSpan.FromSeconds(1);
        bcl.Append(ts);
        a.Append(ts);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var dt = new DateTime(2000, 01, 01);
        bcl.Append(dt);
        a.Append(dt);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var dto = new DateTimeOffset(new DateTime(2000, 01, 01));
        bcl.Append(dto);
        a.Append(dto);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var dec = (decimal)123.4;
        bcl.Append(dec);
        a.Append(dec);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var guid = Guid.Parse("39374ABF-B038-4E00-8E4A-B77672300F94");
        bcl.Append(guid);
        a.Append(guid);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var booleanN = (bool?)true;
        bcl.Append(booleanN);
        a.Append(booleanN);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var int8N = (sbyte?)2;
        bcl.Append(int8N);
        a.Append(int8N);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var int16N = (short?)22;
        bcl.Append(int16N);
        a.Append(int16N);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var int32N = (int?)1234567;
        bcl.Append(int32N);
        a.Append(int32N);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var int64N = (long?)int.MaxValue + 123456;
        bcl.Append(int64N);
        a.Append(int64N);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var byteN = (byte?)0x2;
        bcl.Append(byteN);
        a.Append(byteN);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var uint16N = (ushort?)22;
        bcl.Append(uint16N);
        a.Append(uint16N);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var uint32N = (uint?)1234567;
        bcl.Append(uint32N);
        a.Append(uint32N);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var uint64N = (ulong?)int.MaxValue + 123456;
        bcl.Append(uint64N);
        a.Append(uint64N);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var singleN = (float?)1.1;
        bcl.Append(singleN);
        a.Append(singleN);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var doubleN = (double?)1.1;
        bcl.Append(doubleN);
        a.Append(doubleN);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var tsN = (TimeSpan?)TimeSpan.FromSeconds(1);
        bcl.Append(tsN);
        a.Append(tsN);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var dtN = (DateTime?)new DateTime(2000, 01, 01);
        bcl.Append(dtN);
        a.Append(dtN);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var dtoN = (DateTimeOffset?)new DateTimeOffset(new DateTime(2000, 01, 01));
        bcl.Append(dtoN);
        a.Append(dtoN);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var decN = (decimal?)123.4;
        bcl.Append(decN);
        a.Append(decN);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var guidN = (Guid?)Guid.Parse("39374ABF-B038-4E00-8E4A-B77672300F94");
        bcl.Append(guidN);
        a.Append(guidN);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var nint32 = (nint)1234567;
        bcl.Append(nint32);
        a.Append(nint32);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var nuint32 = (nuint)1234567;
        bcl.Append(nuint32);
        a.Append(nuint32);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var nint32N = (nint)1234567;
        bcl.Append(nint32N);
        a.Append(nint32N);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var nuint32N = (nuint)1234567;
        bcl.Append(nuint32N);
        a.Append(nuint32N);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var intPtr = default(IntPtr);
        bcl.Append(intPtr);
        a.Append(intPtr);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var uintPtr = default(UIntPtr);
        bcl.Append(uintPtr);
        a.Append(uintPtr);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());

        var testString =
            "\u0030\u0031\u0032\u0033\u0054\u0065\u0073\u0074\u6e2c\u8a66\u1e87\u0353\u031e\u0352\u035f\u0361\u01eb\u0320\u0320\u0309\u030f\u0360\u0361\u0345\u0072\u032c\u033a\u035a\u030d\u035b\u0314\u0352\u0362\u0064\u0320\u034e\u0317\u0333\u0347\u0346\u030b\u030a\u0342\u0350\ud83d\udeb5\ud83c\udffb\u200d\u2640\ufe0f\u0022";
        bcl.Append(testString);
        a.Append(testString);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());
    }

    // ─── Append(ValueStringAppender) ─────────────────────────────────────────

    [Test]
    public async Task AppendValueStringAppender()
    {
        using var sb1 = new ValueStringAppender("hello");
        using var sb2 = new ValueStringAppender();
        sb2.Append(sb1);
        await Assert.That(sb2.ToString()).IsEqualTo("hello");
    }

    [Test]
    public async Task AppendValueStringAppenderToExisting()
    {
        using var sb1 = new ValueStringAppender("world");
        using var sb2 = new ValueStringAppender("hello ");
        sb2.Append(sb1);
        await Assert.That(sb2.ToString()).IsEqualTo("hello world");
    }

    [Test]
    public async Task AppendEmptyValueStringAppender()
    {
        using var sb1 = new ValueStringAppender();
        using var sb2 = new ValueStringAppender("hello");
        sb2.Append(sb1);
        await Assert.That(sb2.ToString()).IsEqualTo("hello");
    }

    // ─── Append(ReadOnlyMemory<char>) ─────────────────────────────────────────

    [Test]
    public async Task AppendReadOnlyMemory()
    {
        using var sb  = new ValueStringAppender();
        var       mem = "hello".AsMemory();
        sb.Append(mem);
        await Assert.That(sb.ToString()).IsEqualTo("hello");
    }

    [Test]
    public async Task AppendReadOnlyMemoryEmpty()
    {
        using var sb  = new ValueStringAppender("hi");
        var       mem = ReadOnlyMemory<char>.Empty;
        sb.Append(mem);
        await Assert.That(sb.ToString()).IsEqualTo("hi");
    }

    [Test]
    public async Task AppendReadOnlyMemorySlice()
    {
        using var sb  = new ValueStringAppender();
        var       mem = "hello world".AsMemory(6, 5);
        sb.Append(mem);
        await Assert.That(sb.ToString()).IsEqualTo("world");
    }

    // ─── Append(char[], int, int) ─────────────────────────────────────────────

    [Test]
    public async Task AppendCharArrayWithRange()
    {
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        var       arr = "hello world".ToCharArray();
        sb.Append(arr, 6, 5);
        bcl.Append(arr, 6, 5);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendCharArrayFromStart()
    {
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        var       arr = "hello".ToCharArray();
        sb.Append(arr, 0, arr.Length);
        bcl.Append(arr, 0, arr.Length);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendNullCharArrayWithRange()
    {
        using var sb = new ValueStringAppender("test");
        sb.Append((char[]?)null, 0, 0);
        await Assert.That(sb.ToString()).IsEqualTo("test");
    }

    // ─── Append(char[]) ───────────────────────────────────────────────────────

    [Test]
    public async Task AppendCharArray()
    {
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        var       arr = "hello".ToCharArray();
        sb.Append(arr);
        bcl.Append(arr);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendNullCharArray()
    {
        using var sb = new ValueStringAppender("test");
        sb.Append(null);
        await Assert.That(sb.ToString()).IsEqualTo("test");
    }

    [Test]
    public async Task AppendEmptyCharArray()
    {
        using var sb = new ValueStringAppender("test");
        sb.Append(Array.Empty<char>());
        await Assert.That(sb.ToString()).IsEqualTo("test");
    }

    // ─── Append(string?, int, int) ────────────────────────────────────────────

    [Test]
    public async Task AppendStringWithRange()
    {
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        sb.Append("hello world", 6, 5);
        bcl.Append("hello world", 6, 5);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendStringWithRangeFromStart()
    {
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        sb.Append("hello world", 0, 5);
        bcl.Append("hello world", 0, 5);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    // ─── Append(char, int) ────────────────────────────────────────────────────

    [Test]
    public async Task AppendCharWithRepeat()
    {
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        sb.Append('a', 5);
        bcl.Append('a', 5);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendCharWithLargeRepeatExceedsStackThreshold()
    {
        // > 1024 forces ArrayPool path
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        sb.Append('x', 2000);
        bcl.Append('x', 2000);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    [Test]
    public async Task AppendCharWithZeroRepeat()
    {
        using var sb = new ValueStringAppender("test");
        sb.Append('a', 0);
        await Assert.That(sb.ToString()).IsEqualTo("test");
    }

    [Test]
    public async Task AppendCharWithNegativeRepeat()
    {
        using var sb = new ValueStringAppender("test");
        sb.Append('a', -1);
        await Assert.That(sb.ToString()).IsEqualTo("test");
    }

    [Test]
    public async Task AppendCharWithRepeatUnicode()
    {
        using var sb  = new ValueStringAppender();
        var       bcl = new StringBuilder();
        sb.Append('\u9bd6', 10);
        bcl.Append('\u9bd6', 10);
        await Assert.That(sb.ToString()).IsEqualTo(bcl.ToString());
    }

    // ─── Chained appends ──────────────────────────────────────────────────────

    [Test]
    public async Task AppendVariousTypesChained()
    {
        using var sb  = new ValueStringAppender();
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