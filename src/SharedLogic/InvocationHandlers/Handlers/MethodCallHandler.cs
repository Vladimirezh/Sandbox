using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using Sandbox.Commands;
using Sandbox.Common;

namespace Sandbox.InvocationHandlers
{
    internal sealed class MethodCallHandler : CallHandler
    {
        private readonly IObservable< Message > _messagesObservable;
        private readonly IPublisher< Message > _messagePublisher;

        public MethodCallHandler( IObservable< Message > messagesObservable, IPublisher< Message > messagePublisher )
        {
            _messagesObservable = messagesObservable;
            _messagePublisher = messagePublisher;
        }

        internal override object HandleServerSideRequest( IMethodCallMessage mcm )
        {
            var methodCallCommand = new MethodCallCommand( mcm.MethodName, mcm.Args );

            var task = _messagesObservable.OfType< MethodCallResultAnswer >().Take( 1 ).Where( it => it.AnswerTo == methodCallCommand.Number ).ToTask();
            _messagePublisher.Publish( methodCallCommand );
            var result = task.Result;
            if ( result == null )
                throw new SandboxTerminatedException();
            if ( result.Exception != null )
                throw result.Exception;
            return result.Result;
        }

        internal override void HandleClientSideRequest( object instance, Message msg )
        {
            if ( msg is MethodCallCommand mcc )
            {
                try
                {
                    var method = instance.GetType().GetMethod( mcc.MethodName, mcc.Arguments.Select( it => it.GetType() ).ToArray() );
                    _messagePublisher.Publish( new MethodCallResultAnswer { AnswerTo = msg.Number, Result = method.Invoke( instance, mcc.Arguments ) } );
                }
                catch ( Exception ex )
                {
                    _messagePublisher.Publish( new MethodCallResultAnswer { Exception = ex } );
                }
            }
            else
            {
                Successor?.HandleClientSideRequest( instance, msg );
            }
        }
    }
}