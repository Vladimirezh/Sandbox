using System;
using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using Sandbox.Commands;
using Sandbox.Common;
using Sandbox.InvocationHandlers;


namespace Sandbox.Server
{
    public class SandboxServer<TInterface, TObject>
        where TInterface : class
        where TObject : class, TInterface, new()
    {
        private IDisposable commandsSubscription;
        private static ProxyGenerator _proxyGenerator;

        public SandboxServer(IObservable<Message> messagesObservable, IPublisher<Message> messagePublisher)
        {
            Guard.IsInterface<TInterface>();
            Guard.NotNull(messagePublisher);
            Guard.NotNull(messagesObservable);

            _proxyGenerator = new ProxyGenerator();
            Instance = _proxyGenerator.CreateInterfaceProxyWithoutTarget<TInterface>(new ServerInvocationHandler(messagesObservable,messagePublisher));
            commandsSubscription = messagesObservable.Subscribe(ExecuteCommand);
            messagePublisher.Publish(new SubscribeToUnexpectedExceptionsCommand());
            messagePublisher.Publish(new CreateObjectOfTypeCommad(typeof(TObject).FullName,
                typeof(TObject).Assembly.Location));
        }

        public TInterface Instance { get; }

        private void ExecuteCommand(Message it)
        {
        }
    }
}