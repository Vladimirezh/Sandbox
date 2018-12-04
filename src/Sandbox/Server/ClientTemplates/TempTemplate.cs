using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;

namespace Sandbox.Server.ClientTemplates
{
    internal class TempTemplate : IClientTemplate
    {
        private readonly string _fileName;
        private readonly Template _template;

        public TempTemplate( Platform platform )
        {
            _fileName = Path.GetTempFileName();
            _template = new Template( platform, _fileName );
        }

        public IDisposable Run( string address )
        {
            return new CompositeDisposable( _template.Run( address ), Disposable.Create( DeleteClientFile ) );
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
    }
}