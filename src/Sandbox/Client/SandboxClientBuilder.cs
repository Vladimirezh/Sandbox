using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Sandbox.Client.TerminatePolicy;
using Sandbox.Commands;
using Sandbox.Common;
using Sandbox.Serializer;

namespace Sandbox.Client
{
    public class SandboxClientBuilder
    {
        public SandboxClientBuilder( string address )
        {
            _address = address;
        }

        private readonly string _address;
        private ISerializer _serializer = new ManualBinarySerializer();
        private PublishedMessagesFormatter _publisher;
        private IObservable< Message > _messages;
        private ITerminatePolicy _terminatePolicy = new ExitPolicy();

        public SandboxClientBuilder WithSerializer( ISerializer serializer )
        {
            _serializer = Guard.NotNull( serializer );
            return this;
        }

        public SandboxClientBuilder WithTerminatePolicy( ITerminatePolicy terminatePolicy )
        {
            _terminatePolicy = terminatePolicy;
            return this;
        }

        public SandboxClient Build()
        {
            var server = new NamedPipeServer( new NamedPipedClientFactory(), _address );
            _publisher = new PublishedMessagesFormatter( server, _serializer );
            _messages = server.Select( it => _serializer.Deserialize( it ) );
            var client = new SandboxClient( _messages, _publisher );
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            client.AddDisposeHandler( Disposable.Create( () => AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException ) );
            return client;
        }

        private void CurrentDomainOnUnhandledException( object sender, UnhandledExceptionEventArgs e )
        {
            _publisher.Publish( new UnexpectedExceptionMessage { Exception = e.ExceptionObject as Exception } );
            _terminatePolicy.Terminate();
        }
    }
}