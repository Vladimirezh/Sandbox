using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Sandbox.Commands;

namespace Sandbox.Serializer
{
    public class BinaryFormatterSerializer : ISerializer
    {
        private readonly BinaryFormatter _formatter = new BinaryFormatter();

        public byte[] Serialize(Message message)
        {
            using (var ms = new MemoryStream())
            {
                _formatter.Serialize(ms, message);
                return ms.ToArray();
            }
        }

        public Message Deserialize(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
                return (Message) _formatter.Deserialize(ms);
        }
    }
}