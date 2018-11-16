using System;
using Sandbox.Client;
using Sandbox.Commands;
using Sandbox.Serializer;

namespace ConsoleAppClientPlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                
                new SandboxClientBuilder("superTest").Build();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                throw;
            }

            Console.ReadKey();
        }
    }

    public class ILS : ISerializer
    {
        private ISerializer _serializer = new BinaryFormatterSerializer();

        public byte[] Serialize(Message message)
        {
            Log(message);
            return _serializer.Serialize(message);
        }

        private static void Log(Message message)
        {
            Console.WriteLine($"Type {message.GetType().FullName}");
        }

        public Message Deserialize(byte[] bytes)
        {
            var message = _serializer.Deserialize(bytes);
            Log(message);
            return message;
        }
    }
}