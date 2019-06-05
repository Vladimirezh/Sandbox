using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;
using System.Runtime.InteropServices;
using Sandbox.Common;

namespace Sandbox.Server.ClientTemplates
{
    public class Template : IClientTemplate, IDisposable
    {
        private readonly string _fileName;
        private readonly string _workingDirectory;

        public Template( Platform platform, string fileName, string workingDirectory, bool withPubKey = false )
        {
            _fileName = Guard.NotNullOrEmpty( fileName, nameof( fileName ) );
            _workingDirectory = Guard.NotNullOrEmpty( workingDirectory, nameof( workingDirectory ) );
            ClientGenerator.CreateClient( platform, _fileName, withPubKey );
        }

        public Template( Platform platform, string fileName ) : this( platform, fileName, Environment.CurrentDirectory )
        {
        }

        public Template( Platform platform ) : this( platform, Path.GetTempFileName() )
        {
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

        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [Flags]
        public enum ProcessCreationFlags : uint
        {
            ZERO_FLAG = 0x00000000,
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00001000,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000
        }

        [DllImport( "Kernel32.dll" )]
        private static extern bool CreateProcess( string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation );

        public IDisposable Run( string address )
        {
            var job = new Job.Job();
            var pi = new PROCESS_INFORMATION();
            var si = new STARTUPINFO();

            CreateProcess( null, $"{_fileName} \"{address}\" \"{ Path.GetDirectoryName( typeof( EventLoopScheduler ).Assembly.Location ) }\"", IntPtr.Zero, IntPtr.Zero, false, ( uint ) ( ProcessCreationFlags.CREATE_BREAKAWAY_FROM_JOB | ProcessCreationFlags.CREATE_NO_WINDOW ), IntPtr.Zero, null, ref si, out pi );
            job.AddProcess( pi.hProcess );
            return job;
        }
    }
}