using System;
using System.Runtime.Remoting.Messaging;
using Sandbox.Commands;

namespace Sandbox.InvocationHandlers
{
    public abstract class CallHandler
    {
        protected CallHandler Successor { get; set; }
        public abstract object HandleMethodCall( IMethodCallMessage mcm );
        public abstract void HandleMessage( object instance, Message msg );

        public static CallHandler CreateHandlerFor< T >( IObservable< Message > messagesObservable, IPublisher< Message > messagePublisher )
        {
            return CreateHandlerFor( typeof( T ), messagesObservable, messagePublisher );
        }

        public static CallHandler CreateHandlerFor( Type type, IObservable< Message > messagesObservable, IPublisher< Message > messagePublisher )
        {
            return new EventCallHandler( type, messagePublisher ) { Successor = new MethodCallHandler( messagesObservable, messagePublisher ) };
        }
    }
}