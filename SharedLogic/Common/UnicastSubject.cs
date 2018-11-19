using System;
using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;

namespace Sandbox.Common
{
    //Based on https://github.com/akarnokd/reactive-extensions/blob/master/reactive-extensions/UnicastSubject.cs
    public sealed class UnicastSubject<T> : ISubject<T>
    {
        private readonly ConcurrentQueue<T> queue;
        private IObserver<T> observer;
        private int once;
        private int wip;
        private bool done;
        private Exception error;


        public UnicastSubject()
        {
            queue = new ConcurrentQueue<T>();
        }

        public void OnCompleted()
        {
            if (Volatile.Read(ref done) || IsDisposed())
                return;
            Volatile.Write(ref done, true);
            Drain();
        }

        public void OnError(Exception error)
        {
            if (error == null)
                throw new ArgumentNullException(nameof(error));
            if (Volatile.Read(ref done) || IsDisposed() ||
                Interlocked.CompareExchange(ref this.error, error, null) != null)
                return;
            Volatile.Write(ref done, true);
            Drain();
        }

        public void OnNext(T value)
        {
            if (Volatile.Read(ref done) || IsDisposed())
                return;
            queue.Enqueue(value);
            Drain();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));
            if (Interlocked.CompareExchange(ref once, 1, 0) == 0)
            {
                Volatile.Write(ref this.observer, observer);
                Drain();
                return new DisposeObserver(this);
            }

            observer.OnError(new InvalidOperationException(
                "The UnicastSubject allows at most one IObserver to subscribe during its existence"));
            return Disposable.Empty;
        }


        private void Dispose()
        {
            Volatile.Write(ref observer, null);
            Volatile.Write(ref once, 2);
            Drain();
        }


        private bool IsDisposed()
        {
            return Volatile.Read(ref once) == 2;
        }

        private void Drain()
        {
            if (Interlocked.Increment(ref wip) != 1)
            {
                return;
            }

            var missed = 1;

            for (;;)
            {
                var localObserver = Volatile.Read(ref this.observer);

                if (localObserver != null)
                {
                    for (;;)
                    {
                        if (IsDisposed())
                        {
                            ClearQueue();
                            break;
                        }

                        var d = Volatile.Read(ref done);
                        var success = queue.TryDequeue(out var v);
                        var empty = !success;

                        if (d && empty)
                        {
                            var ex = Volatile.Read(ref error);
                            if (ex != null)
                            {
                                localObserver.OnError(ex);
                            }
                            else
                            {
                                localObserver.OnCompleted();
                            }

                            Volatile.Write(ref this.observer, null);
                            Volatile.Write(ref once, 2);
                            break;
                        }

                        if (empty)
                        {
                            break;
                        }

                        localObserver.OnNext(v);
                    }
                }
                else if (IsDisposed())
                {
                    ClearQueue();
                }

                missed = Interlocked.Add(ref wip, -missed);
                if (missed == 0)
                {
                    break;
                }
            }
        }

        private void ClearQueue()
        {
            while (!queue.IsEmpty)
            {
                queue.TryDequeue(out _);
            }
        }

        private sealed class DisposeObserver : IDisposable
        {
            private UnicastSubject<T> parent;

            public DisposeObserver(UnicastSubject<T> parent)
            {
                Volatile.Write(ref this.parent, parent);
            }

            public void Dispose()
            {
                Interlocked.Exchange(ref parent, null)?.Dispose();
            }
        }
    }
}