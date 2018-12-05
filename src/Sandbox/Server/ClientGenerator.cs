using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Text;
using Microsoft.CSharp;

namespace Sandbox.Server
{
    public sealed class ClientGenerator
    {
        private const string ClientCode =
            "using System.Threading;using System.Reflection;using Sandbox.Client;using System;using System.IO;{0}namespace SandboxClient{{ public static class Program {{ private static void Main( string[] args ) {{ var _libFolder = @\"{1}\"; AppDomain.CurrentDomain.AssemblyResolve += ( s, e ) => {{ var name = new AssemblyName( e.Name ).Name;var path = Path.Combine( _libFolder, name + \".dll\" );if ( File.Exists( path ) )return Assembly.LoadFile( path );path = Path.Combine( _libFolder, name + \".exe\" );return File.Exists( path ) ? Assembly.LoadFile( path ) : null; }};  using ( var mre = new ManualResetEvent( false ) ) using ( (( SandboxClientBuilder)Activator.CreateInstance( Assembly.LoadFile( @\"{2}\" ).GetType( \"Sandbox.Client.SandboxClientBuilder\" ), args[ 0 ])).Build() )  mre.WaitOne(); }} }}}} ";

        public ClientGenerator( Platform platform, string fileName, bool sign = false )
        {
            var snkFilePath = string.Empty;

            if ( sign )
            {
                snkFilePath = Path.GetTempFileName();
                File.WriteAllBytes( snkFilePath, Clients.Sandbox );
            }

            try
            {
                var sources = string.Format( ClientCode, sign ? $"[assembly: AssemblyKeyFile( @\"{snkFilePath}\" )]\r\n" : string.Empty, Path.GetDirectoryName( typeof( EventLoopScheduler ).Assembly.Location ),
                    GetType().Assembly.Location );

                using ( var provider = new CSharpCodeProvider() )
                {
                    var cp = new CompilerParameters();

                    cp.ReferencedAssemblies.Add( "System.dll" );
                    cp.ReferencedAssemblies.Add( "Sandbox.dll" );
                    cp.GenerateExecutable = true;
                    cp.OutputAssembly = fileName;
                    cp.GenerateInMemory = false;

                    cp.CompilerOptions = string.Format( CultureInfo.InvariantCulture, " /optimize /t:winexe /errorreport:none  /nowarn:1607  /o+ /checked- /platform:{0} ", platform.ToString().ToLower() );
                    var compilerResults = provider.CompileAssemblyFromSource( cp, sources );

                    if ( compilerResults.Errors.Count <= 0 )
                        return;
                    var stringBuilder = new StringBuilder();
                    foreach ( CompilerError error in compilerResults.Errors )
                        stringBuilder.AppendLine( error.ToString() );
                    throw new Exception( "Failed to generate worker process: " + stringBuilder );
                }
            }
            finally
            {
                if ( sign )
                    try
                    {
                        File.Delete( snkFilePath );
                    }
                    catch
                    {
                    }
            }
        }
    }
}