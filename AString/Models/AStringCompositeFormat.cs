using System.Buffers;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Astra.Text.Models;

[PublicAPI]
public class AStringCompositeFormat
{
    public AStringCompositeFormat(string? format)
    {
        ArgumentNullException.ThrowIfNull(format);

        var buffer     = ArrayPool<Segment>.Shared.Rent(4);
        var enumerator = new FormatSegmentEnumerator(format);
        var i          = 0;

        var argumentFilter = 0L;
        while (enumerator.MoveNext())
        {
            if (i >= buffer.Length)
            {
                var newBuffer = ArrayPool<Segment>.Shared.Rent(buffer.Length * 2);
                Array.Copy(buffer, newBuffer, buffer.Length);
                ArrayPool<Segment>.Shared.Return(buffer);
                buffer = newBuffer;
            }

            var current = enumerator.Current;
            buffer[i++] = current;

            if (current.Type != SegmentType.Argument) { continue; }

            if (current.ArgumentIndex < 0)
            {
                throw new FormatException(); // TODO
            }

            argumentFilter |= 1L << current.ArgumentIndex;
        }

        MinimumArgumentCount = GetMax(argumentFilter);
        Format               = format;
        Segments             = [..buffer.AsSpan(0, i)];

        ArrayPool<Segment>.Shared.Return(buffer);

        static int GetMax(long filter)
        {
            if (filter == 0) { return 0; }

            var i = 0;

            while (filter > 0)
            {
                if (filter >> i > 0 && (filter & (1 << i)) == 0)
                {
                    throw new FormatException(); // TODO
                }

                filter ^= 1 << i++;
            }

            return i;
        }
    }

    internal ImmutableArray<Segment> Segments { get; }

    public string Format               { get; }
    public int    MinimumArgumentCount { get; }

#if NET8_0_OR_GREATER
    [PublicAPI]
    public delegate void SelectParameter<in TState>([InstantHandle] ref ParameterSender sender,
        int parameterIndex,
        TState state);

    [PublicAPI]
    public string CreateString<TState>([RequireStaticDelegate] SelectParameter<TState> selector, TState state)
    {
        var builder = new ValueStringAppender();
        try
        {
            FillBuilder(ref builder, selector, state);
            return builder.ToString();
        }
        finally { builder.Dispose(); }
    }

    [PublicAPI]
    public byte[] CreateUtf8Array<TState>([RequireStaticDelegate] SelectParameter<TState> selector, TState state)
    {
        var builder = new ValueStringAppender();
        try
        {
            FillBuilder(ref builder, selector, state);
            return builder.ToUtf8Array();
        }
        finally { builder.Dispose(); }
    }

    private void FillBuilder<TState>(ref ValueStringAppender builder, SelectParameter<TState> selector, TState state)
    {
        foreach (var segment in Segments)
        {
            switch (segment.Type)
            {
                case SegmentType.Normal:
                    builder.Append(Format.AsSpan()[segment.Range]);
                    break;
                case SegmentType.EscapedOpenBracket:
                    builder.Append('{');
                    break;
                case SegmentType.EscapedCloseBracket:
                    builder.Append('}');
                    break;
                case SegmentType.Argument:
                    var sender = new ParameterSender(ref builder,
                        null,
                        segment.Alignment,
                        Format.AsMemory(segment.FormatRange));

                    try { selector.Invoke(ref sender, segment.ArgumentIndex, state); }
                    catch (Exception ex)
                    {
                        throw new FormatException($"Error formatting argument {segment.ArgumentIndex}", ex);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }


    public ref struct ParameterSender
    {
        private ref      ValueStringAppender  _valueStringBuilder;
        private readonly IFormatProvider?     _formatProvider;
        private readonly int                  _width;
        private readonly ReadOnlyMemory<char> _format;

        internal ParameterSender(ref ValueStringAppender valueStringBuilder,
            IFormatProvider? formatProvider,
            int width,
            ReadOnlyMemory<char> format)
        {
            _formatProvider = formatProvider;
            _width = width;
            _format = format;
            _valueStringBuilder = ref valueStringBuilder;
        }

        [PublicAPI]
        public void Send<T>(T? value)
        {
            _valueStringBuilder.AppendFormatInternal(_formatProvider, value, _width, _format.Span);
        }
    }

#endif
}