using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Common;

// ReSharper disable AccessToDisposedClosure

namespace Sandbox
{
    public class NamedPipeServer : IObservable< byte[] >, IDisposable, IPublisher< byte[] >
    {
        public NamedPipeServer( INamedPipeStreamFactory streamFactory, string address )
        {
            this.streamFactory = Guard.NotNull( streamFactory );
            Address = Guard.NotNullOrEmpty( address, nameof( address ) );
            serverObservable = Observable.Create< byte[] >( ( observer, token ) => Start( observer, token ) ).Publish().RefCount();
        }

        private readonly UnicastSubject< byte[] > publishSubject = new UnicastSubject< byte[] >();
        private readonly IObservable< byte[] > serverObservable;
        private readonly INamedPipeStreamFactory streamFactory;

        public string Address { get; }

        public void Dispose()
        {
            publishSubject?.OnCompleted();
        }

        public IDisposable Subscribe( IObserver< byte[] > observer )
        {
            return serverObservable.Subscribe( observer );
        }

        public void Publish( byte[] message )
        {
            publishSubject.OnNext( message );
        }

        private async Task Start( IObserver< byte[] > observer, CancellationToken token )
        {
            var isException = false;
            try
            {
                var length = new byte[ sizeof( int ) ];
                using ( var stream = streamFactory.CreateStream( Address ) )
                {
                    await stream.ConnectionAsync( token );
                    using ( publishSubject.Subscribe( async message => await SendMessageAsync( stream, message, token ) ) )
                    {
                        while ( !token.IsCancellationRequested )
                        {
                            var count = await stream.ReadAsync( length, 0, length.Length, token );
                            if ( count <= 0 || token.IsCancellationRequested )
                                return;

                            var messageLength = BitConverter.ToInt32( length, 0 );
                            var message = new byte[ messageLength ];
                            count = await stream.ReadAsync( message, 0, messageLength, token );
                            if ( count != messageLength || token.IsCancellationRequested )
                                return;

                            observer.OnNext( message );
                        }
                    }
                }
            }
            catch ( TaskCanceledException )
            {
            }
            catch ( Exception ex )
            {
                isException = true;
                publishSubject.OnError( ex );
                throw;
            }
            finally
            {
                if ( !isException && !token.IsCancellationRequested )
                {
                    var processTerminatedException = new SandboxTerminatedException();
                    publishSubject.OnError( processTerminatedException );
                    throw processTerminatedException;
                }
            }
        }

        private static async Task SendMessageAsync( INamedPipeStream stream, byte[] message, CancellationToken cancellationToken )
        {
            try
            {
                var messageToSend = new byte[ message.Length + sizeof( int ) ];
                Array.Copy( BitConverter.GetBytes( message.Length ), messageToSend, sizeof( int ) );
                Array.Copy( message, 0, messageToSend, sizeof( int ), message.Length );
                await stream.WriteAsync( messageToSend, 0, messageToSend.Length, cancellationToken );
            }
            catch
            {
            }
        }
    }
}