using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using akarnokd.reactive_extensions;
using Sandbox.Common;

namespace Sandbox
{
    public class NamedPipeServer : IObservable<byte[]>, IDisposable
    {
        private readonly INamedPipeStreamFactory streamFactory;
        public string Address { get; }
        private readonly IObservable<byte[]> serverObservable;

        private readonly UnicastSubject<byte[]> publishSubject = new UnicastSubject<byte[]>();

        public NamedPipeServer(INamedPipeStreamFactory streamFactory, string address)
        {
            this.streamFactory = Guard.NotNull(streamFactory);
            Address = Guard.NotNullOrEmpty(address, nameof(address));
            serverObservable = Observable.Create<byte[]>((observer, token) => Start(observer, token)).Publish()
                .RefCount();
        }

        public void Publish(byte[] message)
        {
            publishSubject.OnNext(message);
        }

        private async Task Start(IObserver<byte[]> observer, CancellationToken token)
        {
            var length = new byte[sizeof(int)];
            using (var stream = streamFactory.CreateStream(Address))
            {
                await stream.ConnectionAsync(token);
                using (publishSubject.Subscribe(async message => await SendMessageAsync(stream, message, token)))

                {
                    while (!token.IsCancellationRequested)
                    {
                        var count = await stream.ReadAsync(length, 0, length.Length, token);
                        if (count <= 0)
                            return;

                        var messageLength = BitConverter.ToInt32(length, 0);
                        var message = new byte[messageLength];
                        count = await stream.ReadAsync(message, 0, messageLength, token);
                        if (count != messageLength)
                            return;

                        observer.OnNext(message);
                    }
                }
            }
        }

        private async Task SendMessageAsync(INamedPipeStream stream, byte[] message,
            CancellationToken cancellationToken)
        {
            var messageToSend = new byte[message.Length + sizeof(int)];
            Array.Copy(BitConverter.GetBytes(message.Length), messageToSend, sizeof(int));
            Array.Copy(message, 0, messageToSend, sizeof(int) , message.Length);
            await stream.WriteAsync(messageToSend, 0, messageToSend.Length, cancellationToken);
        }

        public IDisposable Subscribe(IObserver<byte[]> observer)
        {
            return serverObservable.Subscribe(observer);
        }

        public void Dispose()
        {
            publishSubject?.OnCompleted();
        }
    }
}