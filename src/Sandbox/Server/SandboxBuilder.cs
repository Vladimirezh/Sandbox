using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Sandbox.Common;
using Sandbox.Serializer;

namespace Sandbox.Server
{
    public class SandboxBuilder
    {
        private string _address = Guid.NewGuid().ToString();
        private Platform clientPlatform;
        private bool createClient;
        private ISerializer _serializer = new ManualBinarySerializer();
        private string _fileName = Path.GetTempFileName();

        public SandboxBuilder WithSerializer( ISerializer serializer )
        {
            _serializer = Guard.NotNull( serializer );
            return this;
        }

        public SandboxBuilder WithFileName( string fileName )
        {
            _fileName = Guard.NotNullOrEmpty( fileName, nameof( fileName ) );
            return this;
        }

        public SandboxBuilder WithClient( Platform platform )
        {
            createClient = true;
            clientPlatform = platform;
            return this;
        }

        public SandboxBuilder WithoutClient()
        {
            createClient = false;
            return this;
        }

        public SandboxBuilder WithAddress( string address )
        {
            _address = Guard.NotNullOrEmpty( address, nameof( address ) );
            return this;
        }

        public Sandbox< TInterface, TObject > Build< TInterface, TObject >() where TObject : class, TInterface, new() where TInterface : class
        {
            Guard.IsInterface< TInterface >();

            var server = new NamedPipeServer( new NamedPipedServerFactory(), _address );
            var sandbox = new Sandbox< TInterface, TObject >( server.Select( it => _serializer.Deserialize( it ) ), new PublishedMessagesFormatter( server, _serializer ) );
            if ( createClient )
            {
                sandbox.AddDisposeHandler( CreateAndRunClient() );
                sandbox.AddDisposeHandler( Disposable.Create( DeleteClientFile ) );
            }

            return sandbox;
        }

        private void DeleteClientFile()
        {
            try
            {
                Process.Start( new ProcessStartInfo { Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + _fileName + "\"", WindowStyle = ProcessWindowStyle.Hidden, CreateNoWindow = true, FileName = "cmd.exe" } );
            }
            catch
            {
            }
        }

        private Job.Job CreateAndRunClient()
        {
            switch ( clientPlatform )
            {
                case Platform.x86:
                    File.WriteAllBytes( _fileName, Clients.SandboxClient );
                    break;
                case Platform.x64:
                    File.WriteAllBytes( _fileName, Clients.SandboxClientx64 );
                    break;
                case Platform.AnyCPU:
                    File.WriteAllBytes( _fileName, Clients.SandboxClientAnyCPU );
                    break;
            }

            var job = new Job.Job();

            var si = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"\"{_address}\" \"{Path.GetDirectoryName( typeof( EventLoopScheduler ).Assembly.Location )}\"",
                FileName = _fileName,
                WorkingDirectory = Environment.CurrentDirectory
            };
            job.AddProcess( Process.Start( si ).Handle );
            return job;
        }
    }
}