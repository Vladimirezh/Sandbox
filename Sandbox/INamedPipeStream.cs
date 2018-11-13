using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox
{
    public interface INamedPipeStream : IDisposable
    {
        /// <summary>Asynchronously waits for a client to connect to this <see cref="T:System.IO.Pipes.NamedPipeServerStream"></see> object and monitors cancellation requests.</summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous wait operation.</returns>
        Task ConnectionAsync(CancellationToken cancellationToken);

        /// <summary>Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.</summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None"></see>.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the <paramref name="TResult">TResult</paramref> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer">buffer</paramref> is null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset">offset</paramref> or <paramref name="count">count</paramref> is negative.</exception>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset">offset</paramref> and <paramref name="count">count</paramref> is larger than the buffer length.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The stream has been disposed.</exception>
        /// <exception cref="T:System.InvalidOperationException">The stream is currently in use by a previous read operation.</exception>
        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    }
}