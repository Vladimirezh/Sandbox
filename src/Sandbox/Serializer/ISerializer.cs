using Sandbox.Commands;

namespace Sandbox.Serializer
{
    public interface ISerializer
    {
        byte[] Serialize( Message message );
        Message Deserialize( byte[] bytes );
    }
}