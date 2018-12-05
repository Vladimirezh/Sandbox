using System.Reflection;
using Sandbox.Client;
using System;
using System.IO;



namespace SandboxClient
{
    public static class Program
    {
        private static void Main( string[] args )
        {
            var _libFolder = args[ 1 ];
            AppDomain.CurrentDomain.AssemblyResolve += ( s, e ) =>
                                                       {
                                                           var path = Path.Combine( _libFolder, e.Name.Split( ',' )[ 0 ] + ".dll" );
                                                           return File.Exists( path ) ? Assembly.LoadFile( path ) : null;
                                                       };
            using ( new SandboxClientBuilder( args[ 0 ] ).Build() )
                Console.ReadKey();
        }
    }
}