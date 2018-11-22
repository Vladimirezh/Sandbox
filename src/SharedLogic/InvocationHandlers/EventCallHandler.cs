using System;
using System.Runtime.Remoting.Messaging;
using Sandbox.Commands;
using SharedLogic.InvocationHandlers;

namespace Sandbox.InvocationHandlers
{
    internal sealed class EventCallHandler : CallHandler
    {
        private IObservable< Message > _messagesObservable;
        private IPublisher< Message > _messagePublisher;

        public EventCallHandler( IObservable< Message > messagesObservable, IPublisher< Message > messagePublisher )
        {
            _messagesObservable = messagesObservable;
            _messagePublisher = messagePublisher;
        }

        internal override object HandleServerSideRequest( IMethodCallMessage mcm )
        {
            if ( mcm.IsEvent() )
            {
                throw new NotSupportedException();
            }

            if ( Successor == null )
                throw new NotImplementedException();

            return Successor?.HandleServerSideRequest( mcm );
        }

        internal override void HandleClientSideRequest( object instance, Message msg )
        {
            Successor.HandleClientSideRequest( instance, msg );
        }
    }
}