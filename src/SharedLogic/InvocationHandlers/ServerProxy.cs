using System;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading.Tasks;
using Sandbox.Commands;
using Sandbox.Common;

namespace Sandbox.InvocationHandlers
{
    public class ServerProxy<T> : RealProxy
    {
        private IObservable<Message> _messagesObservable;
        private IPublisher<Message> _messagePublisher;
        public ServerProxy() : base(typeof(T))
        {

        }

        public void Initialize(IObservable<Message> messagesObservable, IPublisher<Message> messagePublisher)
        {
            _messagesObservable = messagesObservable;
            _messagePublisher = messagePublisher;

        }

        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = msg as IMethodCallMessage;

            if (methodCall != null)
            {
                try
                {
                    var methodCallCommand = new MethodCallCommand(methodCall.MethodName, methodCall.Args);
                    var task = new TaskCompletionSource<object>();
                    using (_messagesObservable.OfType<MethodCallResultAnswer>()
                        .Where(it => it.AnswerTo == methodCallCommand.Number).Take(1)
                        .Subscribe(it =>
                        {
                            if (it.Exception != null)
                                task.SetException(it.Exception);
                            else
                                task.SetResult(it.Result);
                        }, ex => task.SetException(ex),
                            () =>
                            {
                                if (task.Task.Status == TaskStatus.Running)
                                    task.SetException(new SandboxTerminatedException());
                            }))
                    {
                        _messagePublisher.Publish(methodCallCommand);
                        return new ReturnMessage(task.Task.Result, null, 0, methodCall.LogicalCallContext, methodCall);
                    }
                }
                catch (AggregateException ex)
                {
                    return new ReturnMessage(ex.InnerException, methodCall);
                }
                catch (Exception ex)
                {
                    return new ReturnMessage(ex, methodCall);
                }
            }
            return null;
        }

        public static T Create(IObservable<Message> messagesObservable, IPublisher<Message> messagePublisher)
        {
            Guard.IsInterface<T>();
            var proxy = new ServerProxy<T>();
            proxy.Initialize(messagesObservable, messagePublisher);
            return (T)proxy.GetTransparentProxy();
        }
    }
}