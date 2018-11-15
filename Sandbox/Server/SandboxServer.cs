using System;
using System.Linq;
using System.Reactive.Subjects;
using Sandbox.Commands;
using Sandbox.Common;
using Sandbox.InvocationHandlers;

namespace Sandbox.Server
{
    public class SandboxServer<TInterface, TObject> : IDisposable
        where TInterface : class
        where TObject : class, TInterface, new()
    {
        private readonly Subject<Exception> _exceptionHandlerSubject = new Subject<Exception>();
        private readonly IPublisher<Message> _messagePublisher;
        private readonly IDisposable _commandsSubscription;

        public SandboxServer(IObservable<Message> messagesObservable, IPublisher<Message> messagePublisher)
        {
            _messagePublisher = messagePublisher;
            Guard.IsInterface<TInterface>();
            Guard.NotNull(messagePublisher);
            Guard.NotNull(messagesObservable);
            Instance = ServerProxy<TInterface>.Create(messagesObservable, messagePublisher);
            _commandsSubscription = messagesObservable.Subscribe(ExecuteCommand);
            messagePublisher.Publish(new CreateObjectOfTypeCommad(typeof(TObject).FullName,
                typeof(TObject).Assembly.Location));
        }

        public IObservable<Exception> UnexpectedExceptionHandler => _exceptionHandlerSubject;

        public TInterface Instance { get; }

        public void Dispose()
        {
            _messagePublisher.Publish(new TerminateCommand());
            _exceptionHandlerSubject?.Dispose();
            _commandsSubscription?.Dispose();
        }

        private void ExecuteCommand(Message it)
        {
            switch (it)
            {
                case UnexpectedExceptionMessage uem:
                    _exceptionHandlerSubject.OnNext(uem.Exception);
                    break;
                case AssemblyResolveMessage arm:
                {
                    var assembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.FullName == arm.RequestingAssemblyFullName);
                    _messagePublisher.Publish(new AssemblyResolveAnswer
                        {Handled = assembly != null, Location = assembly?.Location, AnswerTo = it.Number});
                    if (assembly == null)
                    {
                        _messagePublisher.Publish(new TerminateCommand());
                        _exceptionHandlerSubject.OnNext(
                            new CantResolveAssemblyException(arm.RequestingAssemblyFullName, arm.Name));
                    }

                    _commandsSubscription?.Dispose();

                    break;
                }
            }
        }
    }
}