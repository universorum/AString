using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Astra.Text;

public partial struct ValueStringAppender
{
    [PublicAPI]
    public void AppendLine() { Append(Environment.NewLine); }

    [PublicAPI]
    public void AppendLine(string? value)
    {
        Append(value);
        Append(Environment.NewLine);
    }

    /// <summary>
    ///     Appends the specified interpolated string followed by the default line terminator to the end of the current
    ///     StringBuilder object.
    /// </summary>
    /// <param name="handler">The interpolated string to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    [PublicAPI]
    public void AppendLine([InterpolatedStringHandlerArgument("")] ref AppendInterpolatedStringHandler handler)
    {
        using (var enumerator = handler.GetChunks())
        {
            while (enumerator.MoveNext()) { Append(enumerator.Current); }
        }

        handler.Dispose();

        Append(Environment.NewLine);
    }

    // ReSharper disable once EntityNameCapturedOnly.Global
    /// <summary>
    ///     Appends the specified interpolated string followed by the default line terminator to the end of the current
    ///     StringBuilder object.
    /// </summary>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    /// <param name="handler">The interpolated string to append.</param>
    /// <returns>A reference to this instance after the append operation has completed.</returns>
    [PublicAPI]
    public void AppendLine(IFormatProvider? provider,
        [InterpolatedStringHandlerArgument("", nameof(provider))] ref AppendInterpolatedStringHandler handler)
    {
        using (var enumerator = handler.GetChunks())
        {
            while (enumerator.MoveNext()) { Append(enumerator.Current); }
        }

        handler.Dispose();

        Append(Environment.NewLine);
    }
}