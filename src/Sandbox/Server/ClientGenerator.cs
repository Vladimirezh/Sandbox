using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Text;
using Microsoft.CSharp;
using Sandbox.Client;

namespace Sandbox.Server
{
    public static class ClientGenerator
    {
        public static void CreateClient( Platform platform, string fileName, bool sign )
        {
            var snkFilePath = string.Empty;

            if ( sign )
            {
                snkFilePath = Path.GetTempFileName();
                File.WriteAllBytes( snkFilePath, Resources.Sandbox );
            }

            try
            {
                var sources = string.Format( Resources.ClientCode, sign ? $"[assembly: AssemblyKeyFile( @\"{snkFilePath}\" )]\r\n" : string.Empty, Path.GetDirectoryName( typeof( SandboxClientBuilder ).Assembly.Location ),
                    Path.GetDirectoryName( typeof( EventLoopScheduler ).Assembly.Location ), typeof( SandboxClientBuilder ).Assembly.Location, typeof( SandboxClientBuilder ).FullName );

                using ( var provider = new CSharpCodeProvider() )
                {
                    var cp = new CompilerParameters();

                    cp.ReferencedAssemblies.Add( "System.dll" );
                    cp.ReferencedAssemblies.Add( "System.Core.dll" );
                    cp.ReferencedAssemblies.Add( "System.Windows.dll" );
                    cp.ReferencedAssemblies.Add( "Microsoft.CSharp.dll" );
                    cp.ReferencedAssemblies.Add( "System.Xml.dll" );
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