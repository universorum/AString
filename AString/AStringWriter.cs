using System.Globalization;
using System.Text;
using JetBrains.Annotations;

namespace Astra.Text;

/// <summary>A <see cref="TextWriter" /> implementation that is backed with <see cref="ValueStringAppender" />.</summary>
/// <remarks>It's important to make sure the writer is always properly disposed of.</remarks>
[PublicAPI]
public sealed class AStringWriter : TextWriter
{
    private ValueStringAppender _builder = new();
    private bool                _disposed;

    /// <summary>Creates a new instance using <see cref="CultureInfo.CurrentCulture" /> as format provider.</summary>
    public AStringWriter() : this(CultureInfo.CurrentCulture) { }

    /// <summary>Creates a new instance with the given format provider.</summary>
    public AStringWriter(IFormatProvider formatProvider) : base(formatProvider) { }

    /// <inheritdoc />
    public override Encoding Encoding { get; } = Encoding.Unicode;
    //= new UnicodeEncoding(false, false); // TODO

    /// <inheritdoc />
    public override void Close() { Dispose(true); }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (_disposed) { return; }

        _disposed = true;
        _builder.Dispose();

        base.Dispose(disposing);
    }

    /// <inheritdoc />
    public override void Write(char value)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));

        _builder.Append(value);
    }

    /// <inheritdoc />
    public override void Write(char[] buffer, int index, int count)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, buffer.Length - index);

        _builder.Append(buffer, index, count);
    }

    /// <inheritdoc />
    public override void Write(string? value)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));

        if (value != null) { _builder.Append(value); }
    }

    /// <inheritdoc />
    public override Task WriteAsync(char value)
    {
        Write(value);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteAsync(string? value)
    {
        Write(value);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteAsync(char[] buffer, int index, int count)
    {
        Write(buffer, index, count);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteLineAsync(char value)
    {
        WriteLine(value);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteLineAsync(string? value)
    {
        WriteLine(value);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteLineAsync(char[] buffer, int index, int count)
    {
        WriteLine(buffer, index, count);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override void Write(bool value)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));

        _builder.Append(value);
    }

    /// <inheritdoc />
    public override void Write(decimal value)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));

        _builder.Append(value);
    }

#if NETSTANDARD2_1_OR_GREATER || NET8_0_OR_GREATER
    /// <inheritdoc />
    public override void Write(ReadOnlySpan<char> buffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));

        _builder.Append(buffer);
    }

    /// <inheritdoc />
    public override void WriteLine(ReadOnlySpan<char> buffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));

        _builder.Append(buffer);
        WriteLine();
    }

    /// <inheritdoc />
    public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));

        if (cancellationToken.IsCancellationRequested) { return Task.FromCanceled(cancellationToken); }

        Write(buffer.Span);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));

        if (cancellationToken.IsCancellationRequested) { return Task.FromCanceled(cancellationToken); }

        WriteLine(buffer.Span);
        return Task.CompletedTask;
    }
#else
    public void Write(ReadOnlySpan<char> buffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));

        _builder.Append(buffer);
    }

    public void WriteLine(ReadOnlySpan<char> buffer)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));

        _builder.Append(buffer);
        WriteLine();
    }

    public Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));

        if (cancellationToken.IsCancellationRequested) { return Task.FromCanceled(cancellationToken); }

        Write(buffer.Span);
        return Task.CompletedTask;
    }

    public Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(_builder));

        if (cancellationToken.IsCancellationRequested) { return Task.FromCanceled(cancellationToken); }

        WriteLine(buffer.Span);
        return Task.CompletedTask;
    }
#endif

    /// <inheritdoc />
    public override Task FlushAsync() => Task.CompletedTask;

    /// <inheritdoc />
    public override string ToString() => _builder.ToString();
}