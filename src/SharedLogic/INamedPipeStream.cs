using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox
{
    public interface INamedPipeStream : IDisposable
    {
        Task ConnectionAsync( CancellationToken cancellationToken );
        Task< int > ReadAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken );
        Task WriteAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken );
        Task FlushAsync( CancellationToken cancellationToken );
    }
}