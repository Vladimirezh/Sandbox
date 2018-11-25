using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Sandbox.Commands;
using Sandbox.Common;
using Sandbox.InvocationHandlers;

namespace Sandbox.Server
{
    public class Sandbox< TInterface, TObject > : IDisposable where TInterface : class where TObject : class, TInterface, new()
    {
        public Sandbox( IObservable< Message > messagesObservable, IPublisher< Message > messagePublisher )
        {
            _messagePublisher = messagePublisher;
            Guard.IsInterface< TInterface >();
            Guard.NotNull( messagePublisher );
            Guard.NotNull( messagesObservable );
            _callHandler = CallHandler.CreateHandlerFor< TInterface >( messagesObservable, messagePublisher );
            Instance = InterfaceProxy< TInterface >.Create( _callHandler );
            _commandsSubscription = messagesObservable.Subscribe( ExecuteCommand );
            messagePublisher.Publish( new CreateObjectOfTypeCommad( typeof( TObject ).FullName, typeof( TObject ).Assembly.Location ) );
        }

        private readonly IDisposable _commandsSubscription;
        private readonly CompositeDisposable _disposeHandlers = new CompositeDisposable();
        private readonly Subject< Exception > _exceptionHandlerSubject = new Subject< Exception >();
        private readonly IPublisher< Message > _messagePublisher;
        private readonly CallHandler _callHandler;

        public IObservable< Exception > UnexpectedExceptionHandler => _exceptionHandlerSubject;

        public TInterface Instance { get; }

        public void Dispose()
        {
            _messagePublisher.Publish( new TerminateCommand() );
            _disposeHandlers.Dispose();
            _exceptionHandlerSubject?.Dispose();
            _commandsSubscription?.Dispose();
        }

        public void AddDisposeHandler( IDisposable disposable )
        {
            _disposeHandlers.Add( disposable );
        }

        private void ExecuteCommand( Message it )
        {
            switch ( it )
            {
                case UnexpectedExceptionMessage uem:
                    _exceptionHandlerSubject.OnNext( uem.Exception );
                    break;
                case AssemblyResolveMessage arm:
                {
                    var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault( a => a.FullName == arm.RequestingAssemblyFullName );
                    _messagePublisher.Publish( new AssemblyResolveAnswer { Handled = assembly != null, Location = assembly?.Location, AnswerTo = it.Number } );
                    if ( assembly == null )
                    {
                        _messagePublisher.Publish( new TerminateCommand() );
                        _exceptionHandlerSubject.OnNext( new CantResolveAssemblyException( arm.RequestingAssemblyFullName, arm.Name ) );
                    }

                    _commandsSubscription?.Dispose();

                    break;
                }
                default:
                    _callHandler.HandleMessage( Instance, it );
                    break;
            }
        }
    }
}