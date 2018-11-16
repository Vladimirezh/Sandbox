using System;
using Sandbox.Commands;
using Sandbox.Serializer;
using Sandbox.Server;

namespace ConsoleAppServerPlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var sandbox = new SandboxBuilder().WithAddress("superTest")
                .Build<ISuperTestEx, SuperTestEx>();
            sandbox.UnexpectedExceptionHandler.Subscribe(it => Console.WriteLine(it));
            using (sandbox)
            {
                for (int i = 0; i < 10000; i++)
                {
                    Console.WriteLine("Call void");
                    sandbox.Instance.VoidTest();
                    Console.WriteLine("Call int");
                    Console.WriteLine(sandbox.Instance.IntTest());
                    Console.WriteLine("Call string");
                    Console.WriteLine(sandbox.Instance.StringTest());
                }

                Console.ReadKey();
            }
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

    public interface ISuperTestEx
    {
        void VoidTest();
        int IntTest();
        string StringTest();
    }

    public class SuperTestEx : ISuperTestEx
    {
        public void VoidTest()
        {
            Console.WriteLine("Void methodCall");
        }

        private int i = 0;

        public int IntTest()
        {
            return i++;
        }

        public string StringTest()
        {
            var str = Guid.NewGuid().ToString();
            Console.WriteLine(str);
            return str;
        }
    }
}