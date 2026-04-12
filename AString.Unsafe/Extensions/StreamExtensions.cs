using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System;

internal static class StreamExtensions
{
    extension(Stream self)
    {
#if !NETSTANDARD2_1_OR_GREATER
        public async Task WriteAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            var array = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(array);
                await self.WriteAsync(array, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            }
            finally { ArrayPool<byte>.Shared.Return(array); }
        }
#endif
    }
}