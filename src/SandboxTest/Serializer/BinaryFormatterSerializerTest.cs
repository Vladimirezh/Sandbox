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


            CheckSerialization(serializer, new Message());
            CheckSerialization(serializer, new AssemblyResolveAnswer());
            CheckSerialization(serializer, new AssemblyResolveMessage());
            var notEmptyString = "test";
            CheckSerialization(serializer, new CreateObjectOfTypeCommad(notEmptyString, notEmptyString));
            CheckSerialization(serializer, new MethodCallCommand(notEmptyString, new object[0]));
            CheckSerialization(serializer, new MethodCallResultAnswer());
            CheckSerialization(serializer, new TerminateCommand());
            CheckSerialization(serializer, new UnexpectedExceptionMessage());
        }

        private static void CheckSerialization(BinaryFormatterSerializer serializer, Message message)
        {
            var mess = serializer.Deserialize(serializer.Serialize(message));
            Assert.Equal(mess.Number, message.Number);
        }
    }
}