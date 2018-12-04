using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Sandbox.Common;
using Sandbox.Serializer;
using Sandbox.Server.ClientTemplates;

namespace Sandbox.Server
{
    public sealed class SandboxBuilder
    {
        private string _address = Guid.NewGuid().ToString();
        private ISerializer _serializer = new ManualBinarySerializer();

        private readonly IClientTemplate _template;

        public SandboxBuilder( IClientTemplate template )
        {
            _template = Guard.NotNull( template );
        }

        public SandboxBuilder( Platform platform ) : this( new TempTemplate( platform ) )
        {
        }

        public SandboxBuilder WithSerializer( ISerializer serializer )
        {
            _serializer = Guard.NotNull( serializer );
            return this;
        }

        public SandboxBuilder WithAddress( string address )
        {
            _address = Guard.NotNullOrEmpty( address, nameof( address ) );
            return this;
        }

        public Sandbox< TInterface, TObject > Build< TInterface, TObject >() where TObject : class, TInterface, new() where TInterface : class
        {
            Guard.IsInterface< TInterface >();

            var server = new NamedPipeServer( new NamedPipedServerFactory(), _address );
            var sandbox = new Sandbox< TInterface, TObject >( server.Select( it => _serializer.Deserialize( it ) ), new PublishedMessagesFormatter( server, _serializer ) );
            sandbox.AddDisposeHandler( _template.Run( _address ) ?? Disposable.Empty );

            return sandbox;
        }
    }
}