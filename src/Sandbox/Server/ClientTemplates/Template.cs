﻿using System;
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

        public Template( Platform platform, string fileName ) : this( platform, fileName, Environment.CurrentDirectory )
        {
        }

        public Template( Platform platform ) : this( platform, Path.GetTempFileName() )
        {
        }

        private void CreateFile()
        {
            new ClientGenerator( _platform, _fileName ,true);
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
                WorkingDirectory = _workingDirectory
            };
            job.AddProcess( Process.Start( si ).Handle );
            return job;
        }
    }
}