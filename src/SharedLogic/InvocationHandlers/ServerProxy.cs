using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using Sandbox.Commands;
using Sandbox.Common;

namespace Sandbox.InvocationHandlers
{
    public sealed class ServerProxy< T > : RealProxy
    {
        public ServerProxy() : base( typeof( T ) )
        {
        }

        private IPublisher< Message > _messagePublisher;
        private IObservable< Message > _messagesObservable;
        private CallHandler _callHandler;

        public void Initialize( IObservable< Message > messagesObservable, IPublisher< Message > messagePublisher )
        {
            _callHandler = new EventCallHandler( messagesObservable, messagePublisher ) { Successor = new MethodCallHandler( messagesObservable, messagePublisher ) };
            _messagesObservable = messagesObservable;
            _messagePublisher = messagePublisher;
        }

        public override IMessage Invoke( IMessage msg )
        {
            if ( !( msg is IMethodCallMessage methodCall ) )
                return null;
            try
            {
                return new ReturnMessage( _callHandler.HandleServerSideRequest( methodCall ), null, 0, methodCall.LogicalCallContext, methodCall );
            }
            catch ( AggregateException ex )
            {
                return new ReturnMessage( ex.InnerException, methodCall );
            }
            catch ( Exception ex )
            {
                return new ReturnMessage( ex, methodCall );
            }
        }

        public static T Create( IObservable< Message > messagesObservable, IPublisher< Message > messagePublisher )
        {
            Guard.IsInterface< T >();
            var proxy = new ServerProxy< T >();
            proxy.Initialize( messagesObservable, messagePublisher );
            return ( T ) proxy.GetTransparentProxy();
        }
    }
}