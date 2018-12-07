using System.Threading;
using System.Reflection;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

namespace SandboxClient
{
    public static class Program
    {
        public static void Main( string[] args )
        {
            const string _libFolder = @"";
            var libs = new[] { _libFolder, @"", Environment.CurrentDirectory }.Where( it => !string.IsNullOrEmpty( it ) ).Distinct().ToArray();
            var cache = new ConcurrentDictionary< string, Assembly >();

            ResolveEventHandler currentDomainOnAssemblyResolve = ( s, e ) =>
                                                                 {
                                                                     Assembly assembly;
                                                                     if ( cache.TryGetValue( e.Name, out assembly ) )
                                                                         return assembly;

                                                                     var name = new AssemblyName( e.Name ).Name;
                                                                     var path = libs.SelectMany( it => new[] { Path.Combine( it, name + ".dll" ), Path.Combine( it, name + ".dll" ) } ).FirstOrDefault( it => File.Exists( it ) );
                                                                     if ( path == null )
                                                                         return null;
                                                                     assembly = Assembly.LoadFile( path );
                                                                     cache.TryAdd( e.Name, assembly );
                                                                     return assembly;
                                                                 };
            AppDomain.CurrentDomain.AssemblyResolve += currentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += currentDomainOnAssemblyResolve;

            var type = Assembly.LoadFile( Path.Combine( _libFolder, "Sandbox.dll" ) ).GetType( "Sandbox.Client.SandboxClientBuilder" );

            using ( var mre = new ManualResetEvent( false ) )

            using ( ( Activator.CreateInstance( type, args[ 0 ] ) as dynamic ).Build() )
                mre.WaitOne();
        }
    }
}