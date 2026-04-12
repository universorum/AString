namespace Astra.Text.Models;

internal ref struct FormatSegmentEnumerator(ReadOnlySpan<char> format)
{
    private readonly ReadOnlySpan<char> _format      = format;
    private          int                _i           = 0;
    private          int                _normalStart = -1;

    public bool MoveNext()
    {
        while (_i < _format.Length)
        {
            var c = _format[_i];

            switch (c)
            {
                case '{' when _i + 1 < _format.Length && _format[_i + 1] == '{':
                    if (_normalStart >= 0)
                    {
                        Current      = new Segment { Type = SegmentType.Normal, Range = new Range(_normalStart, _i) };
                        _normalStart = -1;
                        return true;
                    }

                    Current =  new Segment { Type = SegmentType.EscapedOpenBracket };
                    _i      += 2;
                    return true;

                case '{':
                {
                    if (_normalStart >= 0)
                    {
                        Current      = new Segment { Type = SegmentType.Normal, Range = new Range(_normalStart, _i) };
                        _normalStart = -1;
                        return true;
                    }

                    _i++; // skip '{'

                    var indexStart = _i;
                    while (_i < _format.Length && _format[_i] >= '0' && _format[_i] <= '9') { _i++; }

                    if (_i == indexStart)
                    {
                        throw new FormatException($"Invalid format string: expected argument index at position {_i}.");
                    }

                    var argumentIndex = int.Parse(_format[indexStart.._i]);

                    var alignment = 0;
                    if (_i < _format.Length && _format[_i] == ',')
                    {
                        _i++;
                        var neg = _i < _format.Length && _format[_i] == '-';
                        if (neg) { _i++; }

                        var start = _i;
                        while (_i < _format.Length && char.IsDigit(_format[_i])) { _i++; }

                        alignment = int.Parse(_format[start.._i]);
                        if (neg) { alignment = -alignment; }
                    }

                    Range fmt = default;
                    if (_i < _format.Length && _format[_i] == ':')
                    {
                        _i++;
                        var fmtStart = _i;
                        while (_i < _format.Length && _format[_i] != '}')
                        {
                            if (_format[_i] == '\\') { _i++; } // skip escaped char

                            if (_i < _format.Length && _format[_i] != '}') { _i++; }
                        }

                        fmt = fmtStart.._i;
                    }

                    if (_i >= _format.Length || _format[_i] != '}')
                    {
                        throw new FormatException(
                            $"Invalid format string: unclosed '{{' or unexpected character at position {_i}.");
                    }

                    _i++; // skip '}'
                    Current = new Segment
                    {
                        Type          = SegmentType.Argument,
                        ArgumentIndex = argumentIndex,
                        Alignment     = alignment,
                        FormatRange   = fmt
                    };
                    return true;
                }

                case '}' when _i + 1 >= _format.Length || _format[_i + 1] != '}':
                    throw new FormatException($"Invalid format string: unexpected '}}' at position {_i}.");

                case '}' when _i + 1 < _format.Length && _format[_i + 1] == '}':
                    if (_normalStart >= 0)
                    {
                        Current      = new Segment { Type = SegmentType.Normal, Range = new Range(_normalStart, _i) };
                        _normalStart = -1;
                        return true;
                    }

                    Current =  new Segment { Type = SegmentType.EscapedCloseBracket };
                    _i      += 2;
                    return true;

                default:
                    if (_normalStart < 0) { _normalStart = _i; }

                    _i++;
                    break;
            }
        }

        if (_normalStart < 0) { return false; }

        Current      = new Segment { Type = SegmentType.Normal, Range = new Range(_normalStart, _i) };
        _normalStart = -1;
        return true;
    }

    public void Reset()
    {
        _i           = 0;
        _normalStart = 0;
    }

    public Segment Current { get; private set; }
}