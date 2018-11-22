using System;
using System.Runtime.InteropServices;

namespace Sandbox.Server.Job
{
    public class Job : IDisposable
    {
        public Job()
        {
            handle = CreateJobObject( IntPtr.Zero, null );

            var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION { LimitFlags = 0x2000 };

            var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION { BasicLimitInformation = info };

            var length = Marshal.SizeOf( typeof( JOBOBJECT_EXTENDED_LIMIT_INFORMATION ) );
            var extendedInfoPtr = Marshal.AllocHGlobal( length );
            Marshal.StructureToPtr( extendedInfo, extendedInfoPtr, false );

            if ( !SetInformationJobObject( handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, ( uint ) length ) )
                throw new Exception( $"Unable to set information.  Error: {Marshal.GetLastWin32Error()}" );
        }

        private bool disposed;

        private IntPtr handle;

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        [DllImport( "kernel32.dll", CharSet = CharSet.Unicode )]
        private static extern IntPtr CreateJobObject( IntPtr a, string lpName );

        [DllImport( "kernel32.dll" )]
        private static extern bool SetInformationJobObject( IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength );

        [DllImport( "kernel32.dll", SetLastError = true )]
        private static extern bool AssignProcessToJobObject( IntPtr job, IntPtr process );

        [DllImport( "kernel32.dll", SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        private static extern bool CloseHandle( IntPtr hObject );

        private void Dispose( bool disposing )
        {
            if ( disposed )
                return;
            Close();
            disposed = true;
        }

        public void Close()
        {
            CloseHandle( handle );
            handle = IntPtr.Zero;
        }

        public bool AddProcess( IntPtr processHandle )
        {
            return AssignProcessToJobObject( handle, processHandle );
        }
    }
}