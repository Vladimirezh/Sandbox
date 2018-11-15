using Sandbox.Commands;
using Sandbox.Serializer;

namespace Sandbox.Server
{
    public class PublishedMessagesFormatter : IPublisher<Message>
    {
        private readonly IPublisher<byte[]> _server;
        private readonly ISerializer _serializer;

        public PublishedMessagesFormatter(IPublisher<byte[]> server, ISerializer serializer)
        {
            _server = server;
            _serializer = serializer;
        }

        public void Publish(Message message)
        {
            _server.Publish(_serializer.Serialize(message));
        }
    }
}