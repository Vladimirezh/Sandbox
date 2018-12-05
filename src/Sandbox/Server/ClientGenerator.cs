using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;
using Microsoft.CSharp;

namespace Sandbox.Server
{
    public class ClientGenerator
    {
        private const string clientCode =
            "using System.Reflection;using Sandbox.Client;using System;using System.IO;{0}namespace SandboxClient{{ public static class Program {{ private static void Main( string[] args ) {{ var _libFolder = @\"{1}\"; AppDomain.CurrentDomain.AssemblyResolve += ( s, e ) => {{ var path = Path.Combine( _libFolder, e.Name.Split( \',\' )[ 0 ] + \".dll\" ); return File.Exists( path ) ? Assembly.LoadFile( path ) : null; }}; using ( new SandboxClientBuilder( args[ 0 ] ).Build() ) Console.ReadKey(); }} }}}} ";

        public ClientGenerator( Platform platform, string fileName, bool sign = false )
        {
            var sources = string.Format( clientCode, sign ? "[assembly: AssemblyKeyFile( \"Sandbox.snk\" )]\r\n" : string.Empty, Path.GetDirectoryName( typeof( EventLoopScheduler ).Assembly.Location ) );

            using ( var provider = new CSharpCodeProvider() )
            {
                var cp = new CompilerParameters();

                cp.ReferencedAssemblies.Add( "System.dll" );
                cp.ReferencedAssemblies.Add( "Sandbox.dll" );
                cp.GenerateExecutable = true;
                cp.OutputAssembly = fileName;
                cp.GenerateInMemory = false;

                cp.CompilerOptions = string.Format( CultureInfo.InvariantCulture, " /t:winexe /errorreport:none  /nowarn:1607  /o+ /checked- /platform:{0} ", platform.ToString().ToLower() );
                var compilerResults = provider.CompileAssemblyFromSource( cp, sources );

                if ( compilerResults.Errors.Count > 0 )
                {
                    var stringBuilder = new StringBuilder();
                    foreach ( CompilerError error in compilerResults.Errors )
                        stringBuilder.AppendLine( error.ToString() );
                    throw new Exception( "Failed to generate worker process: " + stringBuilder );
                }
            }
        }
    }
}