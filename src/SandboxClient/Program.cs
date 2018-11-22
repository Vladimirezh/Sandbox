using Sandbox.Client;
using System;
using System.IO;
using System.Reflection;

namespace SandboxClient
{
    public static class Program
    {
        private static Sandbox.Client.SandboxClient client;
        private static string _libFolder;

        private static void Main( string[] args )
        {
            _libFolder = args[ 1 ];
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolveFromLibFolder;
            client = new SandboxClientBuilder( args[ 0 ] ).Build();
            Console.ReadKey();
        }

        private static Assembly CurrentDomainOnAssemblyResolveFromLibFolder( object sender, ResolveEventArgs args )
        {
            var path = Path.Combine( _libFolder, args.Name.Split( ',' )[ 0 ] + ".dll" );
            return File.Exists( path ) ? Assembly.LoadFile( path ) : null;
        }
    }
}