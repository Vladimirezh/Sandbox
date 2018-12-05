using System.Threading;
using System.Reflection;
using Sandbox.Client;
using System;
using System.IO;

[assembly: AssemblyKeyFile( @"C:\Users\Home\AppData\Local\Temp\tmpE654.tmp" )]

namespace SandboxClient
{
    public static class Program
    {
        private static void Main( string[] args )
        {
            var _libFolder = @"C:\Users\Home\Desktop\Sandbox\src\ConsolePlayground\bin\Debug";
            AppDomain.CurrentDomain.AssemblyResolve += ( s, e ) =>
                                                       {
                                                           var name = new AssemblyName( e.Name ).Name;
                                                           var path = Path.Combine( _libFolder, name + ".dll" );
                                                           if ( File.Exists( path ) )
                                                               return Assembly.LoadFile( path );
                                                           path = Path.Combine( _libFolder, name + ".exe" );
                                                           return File.Exists( path ) ? Assembly.LoadFile( path ) : null;
                                                       };
            using ( var mre = new ManualResetEvent( false ) )
           // using ( ( ( SandboxClientBuilder ) Activator.CreateInstance( Assembly.LoadFile( "C:\Users\Home\Desktop\Sandbox\src\ConsolePlayground\bin\Debug\Sandbox.dll" ).GetType( "Sandbox.Client.SandboxClientBuilder" ),
              //  args[ 0 ] ) ).Build() )
                mre.WaitOne();
        }
    }
}