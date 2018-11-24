using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
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
        private ISerializer _serializer = new BinaryFormatterSerializer();
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

            client.AddDisposeHandler( _messages.OfType< TerminateCommand >().Subscribe( it => _terminatePolicy.Terminate(), () => _terminatePolicy.Terminate() ) );

            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
            client.AddDisposeHandler( Disposable.Create( () => AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly ) );
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            client.AddDisposeHandler( Disposable.Create( () => AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException ) );
            return client;
        }

        private void CurrentDomainOnUnhandledException( object sender, UnhandledExceptionEventArgs e )
        {
            _publisher.Publish( new UnexpectedExceptionMessage { Exception = e.ExceptionObject as Exception } );
            _terminatePolicy.Terminate();
        }

        private Assembly ResolveAssembly( object sender, ResolveEventArgs args )
        {
            if ( args.RequestingAssembly == null || _messages == null || _publisher == null )
                return null;

            var resolveMessage = new AssemblyResolveMessage { RequestingAssemblyFullName = args.RequestingAssembly.FullName, Name = args.Name };
            var task = _messages.OfType< AssemblyResolveAnswer >().Where( it => it.AnswerTo == resolveMessage.Number ).Take( 1 ).ToTask();
            _publisher.Publish( resolveMessage );
            var answer = task.Result;
            return answer?.Handled == true ? Assembly.LoadFile( answer.Location ) : null;
        }
    }
}