using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using Sandbox.Commands;
using Sandbox.Common;
using SharedLogic.InvocationHandlers;

namespace Sandbox.InvocationHandlers
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
                    if ( _events[ eventName ].Count == 0 )
                        _messagePublisher.Publish( new SubscribeToEventCommand { EventName = eventName } );
                    _events[ eventName ].Add( ( Delegate ) mcm.Args[ 0 ] );
                }
            }
            else if ( mcm.IsUnsubscribeFromEvent() )
            {
                var eventName = mcm.GetEventName();
                using ( _locker.Lock() )
                {
                    _events[ eventName ].Remove( ( Delegate ) mcm.Args[ 0 ] );
                    if ( _events[ eventName ].Count == 0 )
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
                        var del = _events[ uec.EventName ][ 0 ];
                        _events[ uec.EventName ].Remove( del );
                        eventInfo.RemoveEventHandler( instance, del );
                    }

                    break;
                case EventInvokeCommand eic:
                    List< Delegate > invokeList;

                    using ( _locker.Lock() )
                        invokeList = _events[ eic.EventName ].ToList();

                    invokeList.ForEach( it => it.DynamicInvoke( eic.Args ) );
                    break;
                default:

                    Successor?.HandleMessage( instance, msg );
                    break;
            }
        }

        private void EventHandler( string eventName, object[] args )
        {
            _messagePublisher.Publish( new EventInvokeCommand { EventName = eventName, Args = args } );
        }
    }
}