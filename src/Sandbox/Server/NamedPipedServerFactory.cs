using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.Server
{
    public sealed class NamedPipedServerFactory : INamedPipeStreamFactory
    {
        public INamedPipeStream CreateStream( string address )
        {
            return new NamedPipedServer( address );
        }

        private sealed class NamedPipedServer : INamedPipeStream
        {
            public NamedPipedServer( string address )
            {
                stream = new NamedPipeServerStream( address, PipeDirection.InOut, 2, PipeTransmissionMode.Byte, PipeOptions.Asynchronous );
            }

            private readonly NamedPipeServerStream stream;

            public void Dispose()
            {
                stream.Dispose();
            }

            public Task ConnectionAsync( CancellationToken cancellationToken )
            {
                var task = Task.Run( () => stream.WaitForConnection() );
                task.ConfigureAwait( false );
                return task;
                //  return stream.WaitForConnectionAsync(cancellationToken);
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