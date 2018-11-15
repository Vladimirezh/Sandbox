using System;
using System.Reactive.Linq;
using Sandbox.Common;
using Sandbox.Serializer;

namespace Sandbox.Server
{
    public class SandboxBuilder
    {
        private ISerializer serializer = new BinaryFormatterSerializer();
        private string _address = Guid.NewGuid().ToString();

        public SandboxBuilder WithSerializer(ISerializer serializer)
        {
            this.serializer = Guard.NotNull(serializer);
            return this;
        }

        public SandboxBuilder WithAddress(string address)
        {
            _address = Guard.NotNullOrEmpty(address, nameof(address));
            return this;
        }

        public Sandbox<TInterface, TObject> Build<TInterface, TObject>() where TObject : class, TInterface, new()
            where TInterface : class
        {
            Guard.IsInterface<TInterface>();
            var server = new NamedPipeServer(new NamedPipedServerFactory(), _address);

            return new Sandbox<TInterface, TObject>(server.Select(it => serializer.Deserialize(it)),
                new PublishedMessagesFormatter(server, serializer));
        }
    }
}