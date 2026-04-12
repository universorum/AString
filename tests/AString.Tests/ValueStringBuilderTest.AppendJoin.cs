namespace Astra.Text.Tests;

public class ValueStringBuilderTestAppendJoin
{
    public static IEnumerable<Func<int[]>> JoinIntsTestData()
    {
        yield return () => [];
        yield return () => [1];
        yield return () => [1, 2];
        yield return () => [1, 2, 3];
    }

    [Test]
    [MethodDataSource(nameof(JoinIntsTestData))]
    public async Task CharJoinInts(int[] values)
    {
        var separator = ',';

        using var sb = new ValueStringBuilder();
        sb.AppendJoin(separator, values.AsSpan());

        await Assert.That(sb.ToString()).IsEqualTo(string.Join(separator, values));
    }

    [Test]
    [MethodDataSource(nameof(JoinIntsTestData))]
    public async Task StringJoinInts(int[] values)
    {
        var separator = "_,_";

        using var sb = new ValueStringBuilder();
        sb.AppendJoin(separator, values.AsSpan());

        await Assert.That(sb.ToString()).IsEqualTo(string.Join(separator, values));
    }


    public static IEnumerable<Func<string?[]>> JoinStringsTestData()
    {
        yield return () => [];
        yield return () => ["abc", null, "def"];
        yield return () => [null, "foo", "bar"];
    }

    [Test]
    [MethodDataSource(nameof(JoinStringsTestData))]
    public async Task CharJoinStrings(string?[] values)
    {
        var separator = ',';

        using var sb = new ValueStringBuilder();
        sb.AppendJoin(separator, values.AsSpan());

        await Assert.That(sb.ToString()).IsEqualTo(string.Join(separator, values));
    }

    [Test]
    [MethodDataSource(nameof(JoinStringsTestData))]
    public async Task StringJoinStrings(string?[] values)
    {
        var separator = "_,_";

        using var sb = new ValueStringBuilder();
        sb.AppendJoin(separator, values.AsSpan());

        await Assert.That(sb.ToString()).IsEqualTo(string.Join(separator, values));
    }

    [Test]
    [MethodDataSource(nameof(JoinStringsTestData))]
    public async Task ConcatNullTest(string?[] values)
    {
        var span = values.AsSpan();

        using var sb = new ValueStringBuilder();
        sb.AppendJoin([], span);

        await Assert.That(sb.ToString()).IsEqualTo(string.Concat(span));
    }

    [Test]
    public async Task ConcatHugeString()
    {
        var a = new string('a', 10000);
        var b = new string('b', 1000000);

        ReadOnlySpan<string?> array = [a, b];

        using var sb = new ValueStringBuilder();
        sb.AppendJoin(',', array);

        await Assert.That(sb.ToString()).IsEqualTo(string.Join(',', array));
    }

    // ─── AppendJoin(char, string?) ────────────────────────────────────────────

    [Test]
    public async Task CharSeparatorSingleStringValue()
    {
        using var sb = new ValueStringBuilder();
        sb.AppendJoin(',', "hello");
        // single value – separator is not inserted
        await Assert.That(sb.ToString()).IsEqualTo("hello");
    }

    [Test]
    public async Task CharSeparatorNullStringValue()
    {
        using var sb = new ValueStringBuilder("prefix");
        sb.AppendJoin(',', null);
        await Assert.That(sb.ToString()).IsEqualTo("prefix");
    }

    // ─── AppendJoin(ReadOnlySpan<char>, string?) ──────────────────────────────

    [Test]
    public async Task StringSeparatorSingleStringValue()
    {
        using var sb = new ValueStringBuilder();
        sb.AppendJoin(", ", "world");
        await Assert.That(sb.ToString()).IsEqualTo("world");
    }

    [Test]
    public async Task StringSeparatorNullStringValue()
    {
        using var sb = new ValueStringBuilder("prefix");
        sb.AppendJoin(", ", null);
        await Assert.That(sb.ToString()).IsEqualTo("prefix");
    }

    // ─── AppendJoin(char, IEnumerable<T>) ────────────────────────────────────

    [Test]
    public async Task CharSeparatorIEnumerableInts()
    {
        var       values = new[] { 1, 2, 3 };
        using var sb     = new ValueStringBuilder();
        sb.AppendJoin(',', (IEnumerable<int>)values);
        await Assert.That(sb.ToString()).IsEqualTo(string.Join(',', values));
    }

    [Test]
    public async Task CharSeparatorIEnumerableStrings()
    {
        var       values = new[] { "a", "b", "c" };
        using var sb     = new ValueStringBuilder();
        sb.AppendJoin('-', (IEnumerable<string>)values);
        await Assert.That(sb.ToString()).IsEqualTo(string.Join('-', values));
    }

    [Test]
    public async Task CharSeparatorIEnumerableEmpty()
    {
        using var sb = new ValueStringBuilder();
        sb.AppendJoin(',', (IEnumerable<int>)Array.Empty<int>());
        await Assert.That(sb.ToString()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task CharSeparatorIEnumerableSingleElement()
    {
        using var sb = new ValueStringBuilder();
        sb.AppendJoin(',', (IEnumerable<int>)new[] { 42 });
        await Assert.That(sb.ToString()).IsEqualTo("42");
    }

    // ─── AppendJoin(ReadOnlySpan<char>, IEnumerable<T>) ──────────────────────

    [Test]
    public async Task StringSeparatorIEnumerableInts()
    {
        var       values = new[] { 1, 2, 3 };
        using var sb     = new ValueStringBuilder();
        sb.AppendJoin(", ", (IEnumerable<int>)values);
        await Assert.That(sb.ToString()).IsEqualTo(string.Join(", ", values));
    }

    [Test]
    public async Task StringSeparatorIEnumerableStrings()
    {
        var       values = new[] { "foo", "bar", "baz" };
        using var sb     = new ValueStringBuilder();
        sb.AppendJoin(" | ", (IEnumerable<string>)values);
        await Assert.That(sb.ToString()).IsEqualTo(string.Join(" | ", values));
    }

    [Test]
    public async Task StringSeparatorIEnumerableEmpty()
    {
        using var sb = new ValueStringBuilder();
        sb.AppendJoin(", ", (IEnumerable<string>)Array.Empty<string>());
        await Assert.That(sb.ToString()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task StringSeparatorIEnumerableNullElements()
    {
        var       values = new[] { "a", null, "b" };
        using var sb     = new ValueStringBuilder();
        sb.AppendJoin(", ", (IEnumerable<string?>)values);
        await Assert.That(sb.ToString()).IsEqualTo(string.Join(", ", values));
    }
}