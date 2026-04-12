using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringBuilder
{
    [PublicAPI]
    public void AppendJoin(char separator, string? value) => Append(value);

    [PublicAPI]
    public void AppendJoin<T>(char separator, ReadOnlySpan<T> values)
    {
        ReadOnlySpan<char> s = [separator];
        AppendJoin(s, values);
    }

    [PublicAPI]
    public void AppendJoin<T>(char separator, IEnumerable<T> values)
    {
        ReadOnlySpan<char> s = [separator];
        AppendJoin(s, values);
    }

    [PublicAPI]
    public void AppendJoin(ReadOnlySpan<char> separator, string? value) => Append(value);

    [PublicAPI]
    public void AppendJoin<T>(ReadOnlySpan<char> separator, ReadOnlySpan<T> values)
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
    public void AppendJoin<T>(ReadOnlySpan<char> separator, IEnumerable<T> values)
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