using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Sandbox.Commands;
using Sandbox.Common;

namespace Sandbox.InvocationHandlers.Handlers
{
    internal sealed class EventCallHandler : CallHandler
    {
        public EventCallHandler( Type type, IPublisher< Message > messagePublisher )
        {
            _events = type.GetEvents().ToDictionary( it => it.Name, it => new List< Delegate >() );
            _messagePublisher = messagePublisher;
        }

        private readonly Dictionary< string, List< Delegate > > _events;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private readonly IPublisher< Message > _messagePublisher;

        public override object HandleMethodCall( IMethodCallMessage mcm )
        {
            if ( mcm.IsSubscribeToEvent() )
            {
                var eventName = mcm.GetEventName();
                using ( _locker.Lock() )
                {
                    var delegates = _events[ eventName ];
                    if ( delegates.Count == 0 )
                        _messagePublisher.Publish( new SubscribeToEventCommand { EventName = eventName } );
                    delegates.Add( ( Delegate ) mcm.Args[ 0 ] );
                }
            }
            else if ( mcm.IsUnsubscribeFromEvent() )
            {
                var eventName = mcm.GetEventName();
                using ( _locker.Lock() )
                {
                    var delegates = _events[ eventName ];
                    if ( delegates.Remove( ( Delegate ) mcm.Args[ 0 ] ) && delegates.Count == 0 )
                        _messagePublisher.Publish( new UnsubscribeFromEventCommand { EventName = eventName } );
                }
            }
            else
                return Successor?.HandleMethodCall( mcm );

            return null;
        }

        public override void HandleMessage( object instance, Message msg )
        {
            switch ( msg )
            {
                case SubscribeToEventCommand sec:
                    using ( _locker.Lock() )
                    {
                        var eventInfo = instance.GetType().GetEvent( sec.EventName );
                        var del = DelegateFactory.Create( eventInfo, EventHandler );
                        _events[ sec.EventName ].Add( del );
                        eventInfo.AddEventHandler( instance, del );
                    }

                    break;
                case UnsubscribeFromEventCommand uec:
                    using ( _locker.Lock() )
                    {
                        var eventInfo = instance.GetType().GetEvent( uec.EventName );
                        var delegates = _events[ uec.EventName ];
                        if ( delegates.Count > 0 )
                        {
                            var del = delegates[ 0 ];
                            delegates.Remove( del );
                            eventInfo.RemoveEventHandler( instance, del );
                        }
                    }

                    break;
                case EventInvokeCommand eic:
                    List< Delegate > invokeList;

                    using ( _locker.Lock() )
                        invokeList = _events[ eic.EventName ].ToList();

                    invokeList.ForEach( it => it.DynamicInvoke( eic.Arguments ) );
                    break;
                default:

                    Successor?.HandleMessage( instance, msg );
                    break;
            }
        }

        private void EventHandler( string eventName, object[] args )
        {
            _messagePublisher.Publish( new EventInvokeCommand { EventName = eventName, Arguments = args } );
        }
    }
}