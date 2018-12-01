using System;
using Sandbox.Commands;
using Sandbox.Serializer;
using Xunit;

namespace SandboxTest.Serializer
{
    public class BinaryFormatterSerializerTest
    {
        [Fact]
        public void TesMessagesSerializeAndDeserialize()
        {
            var serializer = new BinaryFormatterSerializer();

            CheckSerialization( serializer, new Message() );
            CheckSerialization( serializer, new AssemblyResolveAnswer() );
            CheckSerialization( serializer, new AssemblyResolveMessage() );
            const string notEmptyString = "test";
            CheckSerialization( serializer, new CreateObjectOfTypeCommad( notEmptyString, notEmptyString ) );
            CheckSerialization( serializer, new MethodCallCommand( notEmptyString, new object[ 0 ], Guid.NewGuid().ToString() ) );
            CheckSerialization( serializer, new MethodCallResultAnswer() );
            CheckSerialization( serializer, new TerminateCommand() );
            CheckSerialization( serializer, new UnexpectedExceptionMessage() );
            CheckSerialization( serializer, new EventCommand() );
            CheckSerialization( serializer, new SubscribeToEventCommand() );
            CheckSerialization( serializer, new UnsubscribeFromEventCommand() );
            CheckSerialization( serializer, new EventInvokeCommand() );
        }

        private static void CheckSerialization( ISerializer serializer, Message message )
        {
            var mess = serializer.Deserialize( serializer.Serialize( message ) );
            Assert.Equal( mess.Number, message.Number );
        }
    }
}