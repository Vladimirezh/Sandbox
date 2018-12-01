using System;
using System.Reactive.Disposables;
using System.Threading;

namespace Sandbox.Common
{
    public static class Extensions
    {
        public static IDisposable Lock( this ReaderWriterLockSlim locker )
        {
            locker.EnterWriteLock();
            return Disposable.Create( locker.ExitWriteLock );
        }
    }
}