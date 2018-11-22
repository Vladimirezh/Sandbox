using Sandbox.Commands;
using Sandbox.Serializer;

namespace Sandbox.Common
{
    public class PublishedMessagesFormatter : IPublisher< Message >
    {
        public PublishedMessagesFormatter( IPublisher< byte[] > server, ISerializer serializer )
        {
            _server = server;
            _serializer = serializer;
        }

        private readonly ISerializer _serializer;
        private readonly IPublisher< byte[] > _server;

        public void Publish( Message message )
        {
            _server.Publish( _serializer.Serialize( message ) );
        }
    }
}