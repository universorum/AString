using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringAppender
{
    [PublicAPI]
    public void AppendJoin(char separator, string?[] values) => AppendJoin([separator], values.AsSpan());

    [PublicAPI]
    public void AppendJoin(ReadOnlySpan<char> separator, string?[] values) => AppendJoin(separator, values.AsSpan());

    [PublicAPI]
    public void AppendJoin(char separator, ReadOnlySpan<string?> values) => AppendJoin([separator], values);

    [PublicAPI]
    public void AppendJoin(ReadOnlySpan<char> separator, ReadOnlySpan<string?> values)
    {
        var isFirst = true;
        foreach (var item in values)
        {
            if (!isFirst) { Append(separator); }

            isFirst = false;

            Append(item);
        }
    }

    [PublicAPI]
    public void AppendJoin(char separator, IEnumerable<string?> values) => AppendJoin([separator], values);

    [PublicAPI]
    public void AppendJoin(ReadOnlySpan<char> separator, IEnumerable<string?> values)
    {
        var isFirst = true;
        foreach (var item in values)
        {
            if (!isFirst) { Append(separator); }

            isFirst = false;

            Append(item);
        }
    }

    [PublicAPI]
    public void AppendJoin<T>(char separator, T?[] values) => AppendJoin([separator], values.AsSpan());

    [PublicAPI]
    public void AppendJoin<T>(ReadOnlySpan<char> separator, T?[] values) => AppendJoin(separator, values.AsSpan());

    [PublicAPI]
    public void AppendJoin<T>(char separator, ReadOnlySpan<T?> values) => AppendJoin([separator], values);

    [PublicAPI]
    public void AppendJoin<T>(ReadOnlySpan<char> separator, ReadOnlySpan<T?> values)
    {
        var isFirst = true;
        foreach (var item in values)
        {
            if (!isFirst) { Append(separator); }

            isFirst = false;

            Append(item);
        }
    }

    [PublicAPI]
    public void AppendJoin<T>(char separator, IEnumerable<T?> values)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
        =>
            AppendJoin([separator], values);

    [PublicAPI]
    public void AppendJoin<T>(ReadOnlySpan<char> separator, IEnumerable<T?> values)
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
    {
        var isFirst = true;
        foreach (var item in values)
        {
            if (!isFirst) { Append(separator); }

            isFirst = false;

            Append(item);
        }
    }
}