using System.Threading;
using System.Reflection;
using System;
using System.IO;

namespace SandboxClient
{
    public static class Program
    {
        public static void Main( string[] args )
        {
            const string _libFolder = @"";
            AppDomain.CurrentDomain.AssemblyResolve += ( s, e ) =>
                                                       {
                                                           var name = new AssemblyName( e.Name ).Name;
                                                           var path = Path.Combine( _libFolder, name + ".dll" );
                                                           if ( File.Exists( path ) )
                                                               return Assembly.LoadFile( path );
                                                           path = Path.Combine( _libFolder, name + ".exe" );
                                                           return File.Exists( path ) ? Assembly.LoadFile( path ) : null;
                                                       };

            var type = Assembly.LoadFile( Path.Combine( _libFolder, "Sandbox.dll" ) ).GetType( "Sandbox.Client.SandboxClientBuilder" );

            using ( var mre = new ManualResetEvent( false ) )

            using ( ( Activator.CreateInstance( type, args[ 0 ] ) as dynamic ).Build() )
                mre.WaitOne();
        }
    }
}