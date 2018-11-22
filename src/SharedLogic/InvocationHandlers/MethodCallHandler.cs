using System;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
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
            var task = new TaskCompletionSource< object >();
            using ( _messagesObservable.OfType< MethodCallResultAnswer >().
                                        Where( it => it.AnswerTo == methodCallCommand.Number ).
                                        Take( 1 ).
                                        Subscribe( it =>
                                                   {
                                                       if ( it.Exception != null )
                                                           task.SetException( it.Exception );
                                                       else
                                                           task.SetResult( it.Result );
                                                   }, ex => task.SetException( ex ), () =>
                                                                                     {
                                                                                         if ( task.Task.Status == TaskStatus.Running )
                                                                                             task.SetException( new SandboxTerminatedException() );
                                                                                     } ) )
            {
                _messagePublisher.Publish( methodCallCommand );
            }

            return task.Task.Result;
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
                Successor.HandleClientSideRequest( instance, msg );
            }
        }
    }
}