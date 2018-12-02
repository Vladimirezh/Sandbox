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
            CheckSerialization( serializer, new CreateObjectOfTypeCommand { AssemblyPath = notEmptyString, TypeFullName = notEmptyString } );
            CheckSerialization( serializer, new MethodCallCommand { MethodName = notEmptyString, Arguments = new object[ 0 ], MethodId = Guid.NewGuid().ToString() } );
            CheckSerialization( serializer, new MethodCallResultAnswer() );
            CheckSerialization( serializer, new TerminateCommand() );
            CheckSerialization( serializer, new UnexpectedExceptionMessage() );
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