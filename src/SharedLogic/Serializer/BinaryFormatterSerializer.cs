using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Sandbox.Commands;

namespace Sandbox.Serializer
{
    public class BinaryFormatterSerializer : ISerializer
    {
        private readonly BinaryFormatter _formatter = new BinaryFormatter { Binder = new Binder() };

        public byte[] Serialize( Message message )
        {
            using ( var ms = new MemoryStream() )
            {
                _formatter.Serialize( ms, message );
                return ms.ToArray();
            }
        }

        public Message Deserialize( byte[] bytes )
        {
            using ( var ms = new MemoryStream( bytes ) )
            {
                return ( Message ) _formatter.Deserialize( ms );
            }
        }

        private sealed class Binder : SerializationBinder
        {
            private readonly string _pubKey;

            public Binder()
            {
                _pubKey = GetType().Assembly.FullName.Split( '=' ).Last();
            }

            public override Type BindToType( string assemblyName, string typeName )
            {
                if ( assemblyName.EndsWith( _pubKey, StringComparison.Ordinal ) )
                    return Assembly.GetExecutingAssembly().GetType( typeName );

                var assembly = Assembly.Load( assemblyName );
                return FormatterServices.GetTypeFromAssembly( assembly, typeName );
            }
        }
    }
}