using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using Sandbox.Commands;
using Sandbox.Common;

namespace Sandbox.InvocationHandlers
{
    public sealed class ServerProxy< T > : RealProxy
    {
        public ServerProxy( CallHandler handler ) : base( typeof( T ) )
        {
            _callHandler = handler;
        }

        public override object GetTransparentProxy()
        {
            var transparentProxy = base.GetTransparentProxy();
            return transparentProxy;
        }

        private readonly CallHandler _callHandler;

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

        public static T Create( CallHandler handler )
        {
            Guard.IsInterface< T >();
            var proxy = new ServerProxy< T >( handler );
            return ( T ) proxy.GetTransparentProxy();
        }
    }
}