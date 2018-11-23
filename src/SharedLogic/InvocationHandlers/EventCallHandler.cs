using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Sandbox.Commands;
using SharedLogic.InvocationHandlers;

namespace Sandbox.InvocationHandlers
{
    internal sealed class EventCallHandler : CallHandler
    {
        private readonly IPublisher<Message> _messagePublisher;
        private readonly Dictionary<string, List<Delegate>> _events;
        private readonly object _locker = new object();

        public EventCallHandler(Type type, IPublisher<Message> messagePublisher)
        {
            _events = type.GetEvents().ToDictionary(it => it.Name, it => new List<Delegate>());
            _messagePublisher = messagePublisher;
        }

        internal override object HandleServerSideRequest(IMethodCallMessage mcm)
        {
            var eventName = mcm.GetEventName();
            if (mcm.IsSubscribeToEvent())
            {
                lock (_locker)
                {
                    if (_events[eventName].Count == 0)
                        _messagePublisher.Publish(new SubscribeToEventCommand { EventName = eventName });
                    _events[eventName].Add((Delegate)mcm.Args[0]);
                }
            }
            else if (mcm.IsUnsubscribeFromEvent())
            {
                lock (_locker)
                {
                    _events[eventName].Remove((Delegate)mcm.Args[0]);
                    if (_events[eventName].Count == 0)
                        _messagePublisher.Publish(new UnsubscribeFromEventCommand { EventName = eventName });
                }
            }            
            else
                return Successor?.HandleServerSideRequest(mcm);
            return null;
        }

        internal override void HandleClientSideRequest(object instance, Message msg)
        {
            switch (msg)
            {
                case SubscribeToEventCommand sec:
                    lock (_locker)
                    {
                        var eventInfo = instance.GetType().GetEvent(sec.EventName);
                        var del = DelegateFactory.Create(eventInfo, EventHandler);
                        _events[sec.EventName].Add(del);
                        eventInfo.AddEventHandler(instance, del);
                    }

                    break;
                case UnsubscribeFromEventCommand uec:
                    lock (_locker)
                    {
                        var eventInfo = instance.GetType().GetEvent(uec.EventName);
                        var del = _events[uec.EventName][0];
                        _events[uec.EventName].Remove(del);
                        eventInfo.RemoveEventHandler(instance, del);
                    }

                    break;
                case EventInvokeCommand eic:
                    List<Delegate> invokeList;

                    lock (_locker)
                        invokeList = _events[eic.EventName].ToList();

                    invokeList.ForEach(it => it.DynamicInvoke(eic.Args));
                    break;
                default:

                    Successor?.HandleClientSideRequest(instance, msg);
                    break;
            }
        }

        private void EventHandler(string eventName, object[] args)
        {
            _messagePublisher.Publish(new EventInvokeCommand { EventName = eventName, Args = args });
        }
    }
}