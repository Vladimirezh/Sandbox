using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using Sandbox.Commands;
using Sandbox.Common;

namespace Sandbox.InvocationHandlers
{
    internal sealed class MethodCallHandler : CallHandler
    {
        public MethodCallHandler( IObservable< Message > messagesObservable, IPublisher< Message > messagePublisher )
        {
            _messagesObservable = messagesObservable;
            _messagePublisher = messagePublisher;
        }

        private readonly IPublisher< Message > _messagePublisher;
        private readonly IObservable< Message > _messagesObservable;
        private readonly Dictionary< string, MethodInfo > clientCache = new Dictionary< string, MethodInfo >();
        private readonly Dictionary< MethodBase, string > serverCache = new Dictionary< MethodBase, string >();

        public override object HandleMethodCall( IMethodCallMessage mcm )
        {
            if ( !serverCache.TryGetValue( mcm.MethodBase, out var methodId ) )
                methodId = serverCache[ mcm.MethodBase ] = Guid.NewGuid().ToString();
            var methodCallCommand = new MethodCallCommand( mcm.MethodName, mcm.Args, methodId );

            var task = _messagesObservable.OfType< MethodCallResultAnswer >().Take( 1 ).Where( it => it.AnswerTo == methodCallCommand.Number ).ToTask();
            _messagePublisher.Publish( methodCallCommand );
            var result = task.Result;
            if ( result == null )
                throw new SandboxTerminatedException();
            if ( result.Exception != null )
                throw result.Exception;
            return result.Result;
        }

        public override void HandleMessage( object instance, Message msg )
        {
            if ( msg is MethodCallCommand mcc )
            {
                try
                {
                    if ( !clientCache.TryGetValue( mcc.MethodId, out var method ) )
                        method = clientCache[ mcc.MethodId ] = instance.GetType().GetMethod( mcc.MethodName, mcc.Arguments.Select( it => it.GetType() ).ToArray() );
                    _messagePublisher.Publish( new MethodCallResultAnswer { AnswerTo = msg.Number, Result = method.Invoke( instance, mcc.Arguments ) } );
                }
                catch ( Exception ex )
                {
                    _messagePublisher.Publish( new MethodCallResultAnswer { Exception = ex } );
                }
            }
            else
            {
                Successor?.HandleMessage( instance, msg );
            }
        }
    }
}