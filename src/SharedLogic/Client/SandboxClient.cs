using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using Sandbox.Commands;
using Sandbox.InvocationHandlers;

namespace Sandbox.Client
{
    public class SandboxClient : IDisposable
    {
        public SandboxClient( IObservable< Message > messages, IPublisher< Message > publisher )
        {
            _messages = messages;
            _publisher = publisher;
            _subscription = messages.ObserveOn( _scheduler ).Subscribe( ExecuteCommands );
        }

        private readonly IObservable< Message > _messages;
        private readonly IPublisher< Message > _publisher;
        private readonly EventLoopScheduler _scheduler = new EventLoopScheduler();
        private readonly IDisposable _subscription;
        private object _instance;
        private CallHandler _callHandler;
        private readonly CompositeDisposable _disposeHandlers = new CompositeDisposable();

        public void AddDisposeHandler( IDisposable disposable )
        {
            _disposeHandlers.Add( disposable );
        }

        public void Dispose()
        {
            _disposeHandlers.Dispose();
            _scheduler?.Dispose();
            _subscription?.Dispose();
        }

        private void ExecuteCommands( Message message )
        {
            switch ( message )
            {
                case CreateObjectOfTypeCommad co:
                    var type = Assembly.LoadFile( co.AssemblyPath ).GetType( co.TypeFullName );
                    _instance = Activator.CreateInstance( type );
                    _callHandler = CallHandler.CreateHandlerFor( type, _messages, _publisher );
                    break;
                default:
                    _callHandler.HandleMessage( _instance, message );
                    break;
            }
        }
    }
}