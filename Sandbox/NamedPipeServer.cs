using System;
using System.IO.Pipes;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Common;

namespace Sandbox
{
    public class NamedPipeServer : IObservable<byte[]>
    {
        private readonly INamedPipeStreamFactory streamFactory;
        public string Address { get; }
        private readonly IObservable<byte[]> serverObservable;

        public NamedPipeServer(INamedPipeStreamFactory streamFactory, string address)
        {
            this.streamFactory = Guard.NotNull(streamFactory);
            Address = Guard.NotNullOrEmpty(address, nameof(address));
            serverObservable = Observable.Create<byte[]>((observer, token) => Start(observer, token)).Publish()
                .RefCount();
        }

        private async Task Start(IObserver<byte[]> observer, CancellationToken token)
        {
            var length = new byte[sizeof(int)];
            using (var server = streamFactory.CreateStream(Address))
            {
                await server.ConnectionAsync(token);
                while (!token.IsCancellationRequested)
                {
                    var count = await server.ReadAsync(length, 0, length.Length, token);
                    if (count <= 0)
                        return;

                    var messageLength = BitConverter.ToInt32(length, 0);
                    var message = new byte[messageLength];
                    count = await server.ReadAsync(message, 0, messageLength, token);
                    if (count != messageLength)
                        return;

                    observer.OnNext(message);
                }
            }
        }

        public IDisposable Subscribe(IObserver<byte[]> observer)
        {
            return serverObservable.Subscribe(observer);
        }
    }
}