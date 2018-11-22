using System.Runtime.Remoting.Messaging;
using Sandbox.Commands;

namespace Sandbox.InvocationHandlers
{
    public abstract class CallHandler
    {
        internal CallHandler Successor { get; set; }
        internal abstract object HandleServerSideRequest( IMethodCallMessage mcm );
        internal abstract void HandleClientSideRequest( object instance, Message msg );
    }
}