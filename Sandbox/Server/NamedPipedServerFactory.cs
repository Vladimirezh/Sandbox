using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Sandbox.Server
{
    public class NamedPipedServerFactory : INamedPipeStreamFactory
    {
        public INamedPipeStream CreateStream(string address)
        {
            return new NamedPipedServer(address);
        }

        private class NamedPipedServer : INamedPipeStream
        {
            private NamedPipeServerStream stream;

            public NamedPipedServer(string address)
            {
                stream = new NamedPipeServerStream(address, PipeDirection.InOut, 10, PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);
            }

            public void Dispose()
            {
                stream.Dispose();
            }

            public Task ConnectionAsync(CancellationToken cancellationToken)
            {
                return stream.WaitForConnectionAsync(cancellationToken);
            }

            public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return stream.ReadAsync(buffer, offset, count, cancellationToken);
            }

            public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return stream.WriteAsync(buffer, offset, count, cancellationToken);
            }
        }
    }
}