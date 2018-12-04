using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;
using Sandbox.Common;

namespace Sandbox.Server.ClientTemplates
{
    public class Template : IClientTemplate, IDisposable
    {
        private readonly Platform _platform;
        private readonly string _fileName;
        private readonly string _workingDirectory;

        public Template( Platform platform, string fileName, string workingDirectory )
        {
            _platform = platform;
            _fileName = Guard.NotNullOrEmpty( fileName, nameof( fileName ) );
            _workingDirectory = Guard.NotNullOrEmpty( workingDirectory, nameof( workingDirectory ) );
            CreateFile();
        }

        public Template( Platform platform, string fileName )
        {
            _platform = platform;
            _fileName = fileName;
            _fileName = Guard.NotNullOrEmpty( fileName, nameof( fileName ) );
            CreateFile();
        }

        public Template( Platform platform )
        {
            _platform = platform;
            _fileName = Path.GetTempFileName();
            CreateFile();
        }

        private void CreateFile()
        {
            switch ( _platform )
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
        }

        public void Dispose()
        {
            try
            {
                File.Delete( _fileName );
            }
            catch
            {
            }
        }

        public IDisposable Run( string address )
        {
            var job = new Job.Job();

            var si = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"\"{address}\" \"{Path.GetDirectoryName( typeof( EventLoopScheduler ).Assembly.Location )}\"",
                FileName = _fileName,
                WorkingDirectory = _workingDirectory ?? Environment.CurrentDirectory
            };
            job.AddProcess( Process.Start( si ).Handle );
            return job;
        }
    }
}