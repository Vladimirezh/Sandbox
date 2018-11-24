using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.Client
{
    internal sealed class NamedPipedClientFactory : INamedPipeStreamFactory
    {
        public INamedPipeStream CreateStream( string address )
        {
            return new NamedPipeSandboxClientStream( address );
        }

        private sealed class NamedPipeSandboxClientStream : INamedPipeStream
        {
            public NamedPipeSandboxClientStream( string address )
            {
                stream = new NamedPipeClientStream( ".", address, PipeDirection.InOut, PipeOptions.Asynchronous );
            }

            private readonly NamedPipeClientStream stream;

            public void Dispose()
            {
                stream.Dispose();
            }

            public Task ConnectionAsync( CancellationToken cancellationToken )
            {
                return Task.Run( () => stream.Connect(), cancellationToken );
            }

            public Task< int > ReadAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
            {
                return stream.ReadAsync( buffer, offset, count, cancellationToken );
            }

            public Task WriteAsync( byte[] buffer, int offset, int count, CancellationToken cancellationToken )
            {
                return stream.WriteAsync( buffer, offset, count, cancellationToken );
            }

            public Task FlushAsync( CancellationToken cancellationToken )
            {
                return stream.FlushAsync( cancellationToken );
            }
        }
    }
}