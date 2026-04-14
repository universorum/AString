using System.Collections.Frozen;
using System.Security.Cryptography;
using System.Text;

namespace Astra.Text.Tests;

public class ValueStringAppenderTestAppendT
{
    private static FrozenDictionary<Type, object> Dict { get; } = new Dictionary<Type, object>
    {
        [typeof(bool)]            = true,
        [typeof(sbyte)]           = (sbyte)1,
        [typeof(byte)]            = (byte)2,
        [typeof(short)]           = (short)4,
        [typeof(int)]             = 8,
        [typeof(long)]            = (long)16,
        [typeof(ushort)]          = (ushort)32,
        [typeof(uint)]            = (uint)64,
        [typeof(ulong)]           = (ulong)128,
        [typeof(float)]           = (float)256.123,
        [typeof(double)]          = 512.456,
        [typeof(decimal)]         = (decimal)1024.789,
        [typeof(bool?)]           = (bool?)true,
        [typeof(sbyte?)]          = (sbyte?)1,
        [typeof(byte?)]           = (byte?)2,
        [typeof(short?)]          = (short?)4,
        [typeof(int?)]            = (int?)8,
        [typeof(long?)]           = (long?)16,
        [typeof(ushort?)]         = (ushort?)32,
        [typeof(uint?)]           = (uint?)64,
        [typeof(ulong?)]          = (ulong?)128,
        [typeof(float?)]          = (float?)256.123,
        [typeof(double?)]         = (double?)512.456,
        [typeof(decimal?)]        = (decimal?)1024.789,
        [typeof(nint)]            = (nint)1,
        [typeof(nuint)]           = (nuint)1,
        [typeof(nint?)]           = (nint?)1,
        [typeof(nuint?)]          = (nuint?)1,
        [typeof(TimeSpan)]        = TimeSpan.FromSeconds(1),
        [typeof(DateTime)]        = new DateTime(200, 1, 1),
        [typeof(DateTimeOffset)]  = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
        [typeof(Guid)]            = Guid.Parse("39374ABF-B038-4E00-8E4A-B77672300F94"),
        [typeof(TimeSpan?)]       = (TimeSpan?)TimeSpan.FromSeconds(1),
        [typeof(DateTime?)]       = (DateTime?)new DateTime(200, 1, 1),
        [typeof(DateTimeOffset?)] = (DateTimeOffset?)new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
        [typeof(Guid?)]           = (Guid?)Guid.Parse("39374ABF-B038-4E00-8E4A-B77672300F94"),
        [typeof(string)] =
            "\u0030\u0031\u0032\u0033\u0054\u0065\u0073\u0074\u6e2c\u8a66\u1e87\u0353\u031e\u0352\u035f\u0361\u01eb\u0320\u0320\u0309\u030f\u0360\u0361\u0345\u0072\u032c\u033a\u035a\u030d\u035b\u0314\u0352\u0362\u0064\u0320\u034e\u0317\u0333\u0347\u0346\u030b\u030a\u0342\u0350\ud83d\udeb5\ud83c\udffb\u200d\u2640\ufe0f\u0022",
#if NET8_0_OR_GREATER
        [typeof(Rune)] = new Rune('\ud83d', '\udd2e'),
        [typeof(Rune?)] = (Rune?)new Rune('\ud83d', '\udd2e'),
        [typeof(DateOnly)] = new DateOnly(2000, 1, 1),
        [typeof(TimeOnly)] = new TimeOnly(12, 0, 0),
        [typeof(DateOnly?)] = (DateOnly?)new DateOnly(2000, 1, 1),
        [typeof(TimeOnly?)] = (TimeOnly?)new TimeOnly(12, 0, 0),
#endif
    }.ToFrozenDictionary();

    // ─── Append(T) ─────────────────────────────────────────

    [Test]
    [GenerateGenericTest(typeof(bool))]
    [GenerateGenericTest(typeof(sbyte))]
    [GenerateGenericTest(typeof(byte))]
    [GenerateGenericTest(typeof(short))]
    [GenerateGenericTest(typeof(int))]
    [GenerateGenericTest(typeof(long))]
    [GenerateGenericTest(typeof(ushort))]
    [GenerateGenericTest(typeof(uint))]
    [GenerateGenericTest(typeof(ulong))]
    [GenerateGenericTest(typeof(float))]
    [GenerateGenericTest(typeof(double))]
    [GenerateGenericTest(typeof(decimal))]
    [GenerateGenericTest(typeof(bool?))]
    [GenerateGenericTest(typeof(sbyte?))]
    [GenerateGenericTest(typeof(byte?))]
    [GenerateGenericTest(typeof(short?))]
    [GenerateGenericTest(typeof(int?))]
    [GenerateGenericTest(typeof(long?))]
    [GenerateGenericTest(typeof(ushort?))]
    [GenerateGenericTest(typeof(uint?))]
    [GenerateGenericTest(typeof(ulong?))]
    [GenerateGenericTest(typeof(float?))]
    [GenerateGenericTest(typeof(double?))]
    [GenerateGenericTest(typeof(decimal?))]
    [GenerateGenericTest(typeof(nint))]
    [GenerateGenericTest(typeof(nuint))]
    [GenerateGenericTest(typeof(nint?))]
    [GenerateGenericTest(typeof(nuint?))]
    [GenerateGenericTest(typeof(TimeSpan))]
    [GenerateGenericTest(typeof(DateTime))]
    [GenerateGenericTest(typeof(DateTimeOffset))]
    [GenerateGenericTest(typeof(Guid))]
    [GenerateGenericTest(typeof(TimeSpan?))]
    [GenerateGenericTest(typeof(DateTime?))]
    [GenerateGenericTest(typeof(DateTimeOffset?))]
    [GenerateGenericTest(typeof(Guid?))]
    [GenerateGenericTest(typeof(string))]
#if NET8_0_OR_GREATER
    [GenerateGenericTest(typeof(Rune))]
    [GenerateGenericTest(typeof(Rune?))]
    [GenerateGenericTest(typeof(DateOnly))]
    [GenerateGenericTest(typeof(TimeOnly))]
    [GenerateGenericTest(typeof(DateOnly?))]
    [GenerateGenericTest(typeof(TimeOnly?))]
#endif
    public async Task AppendT<T>()
    {
        var value = (T)Dict[typeof(T)];

        var       bcl = new StringBuilder();
        using var a   = new ValueStringAppender();

        bcl.Append(value);
        a.Append<T>(value);
        await Assert.That(bcl.ToString()).IsEquatableTo(a.ToString());
    }

    [Test]
    [GenerateGenericTest(typeof(bool?))]
    [GenerateGenericTest(typeof(sbyte?))]
    [GenerateGenericTest(typeof(byte?))]
    [GenerateGenericTest(typeof(short?))]
    [GenerateGenericTest(typeof(int?))]
    [GenerateGenericTest(typeof(long?))]
    [GenerateGenericTest(typeof(ushort?))]
    [GenerateGenericTest(typeof(uint?))]
    [GenerateGenericTest(typeof(ulong?))]
    [GenerateGenericTest(typeof(float?))]
    [GenerateGenericTest(typeof(double?))]
    [GenerateGenericTest(typeof(decimal?))]
    [GenerateGenericTest(typeof(nint?))]
    [GenerateGenericTest(typeof(nuint?))]
    [GenerateGenericTest(typeof(TimeSpan?))]
    [GenerateGenericTest(typeof(DateTime?))]
    [GenerateGenericTest(typeof(DateTimeOffset?))]
    [GenerateGenericTest(typeof(Guid?))]
    [GenerateGenericTest(typeof(string))]
#if NET8_0_OR_GREATER
    [GenerateGenericTest(typeof(Rune?))]
    [GenerateGenericTest(typeof(DateOnly?))]
    [GenerateGenericTest(typeof(TimeOnly?))]
#endif
    public async Task AppendTDefault<T>()
    {
        var       bcl = new StringBuilder();
        using var a   = new ValueStringAppender();

        a.Append<T>(default!);
        await Assert.That(bcl.ToString()).IsEquatableTo(string.Empty);
    }


    [Test]
    public async Task AppendTLarge()
    {
        var large = new LargeObject(8192);

        using var a = new ValueStringAppender();

        a.Append(large);
        await Assert.That(a.ToString()).IsEquatableTo(large.Value);
    }


    class LargeObject
    {
        private const           int    Seed   = 1337;
        private static readonly Random Random = new(Seed);

        public string Value { get; }

        public override string ToString() => Value;

        public LargeObject(int length)
        {
            Value = string.Create(length,
                Random,
                static (span, random) =>
                {
                    while (!span.IsEmpty)
                    {
                        var next = random.Next(int.MinValue, int.MaxValue);
                        if (!next.TryFormat(span, out var written)) { break; }

                        span = span[written..];
                    }

                    span.Fill('x');
                });
        }
    }
}