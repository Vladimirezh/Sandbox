using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Sandbox.Commands;

namespace Sandbox.Serializer
{
    public class BinaryFormatterSerializer : ISerializer
    {
        private readonly BinaryFormatter _formatter = new BinaryFormatter { Binder = new Binder() };

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
            {
                return (Message)_formatter.Deserialize(ms);
            }
        }

        private sealed class Binder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {//TODO
                if (assemblyName.Contains("339c247525941d52"))
                    return Assembly.GetExecutingAssembly().GetType(typeName);

                var assembly = Assembly.Load(assemblyName);
                Console.WriteLine($"{typeName},{assemblyName}");
                return FormatterServices.GetTypeFromAssembly(assembly, typeName);
            }
        }
    }
}