using System;
using System.Runtime.Remoting.Messaging;
using Sandbox.Commands;

namespace Sandbox.InvocationHandlers
{
    public abstract class CallHandler
    {
        internal CallHandler Successor { get; set; }
        internal abstract object HandleServerSideRequest( IMethodCallMessage mcm );
        internal abstract void HandleClientSideRequest( object instance, Message msg );

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