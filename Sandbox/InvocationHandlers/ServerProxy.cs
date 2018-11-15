using System;
using System.Reactive.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Sandbox.Commands;
using Sandbox.Common;

namespace Sandbox.InvocationHandlers
{
    public class ServerProxy<T> : DispatchProxy
    {
        private IObservable<Message> _messagesObservable;
        private IPublisher<Message> _messagePublisher;

        public void Initialize(IObservable<Message> messagesObservable, IPublisher<Message> messagePublisher)
        {
            _messagesObservable = messagesObservable;
            _messagePublisher = messagePublisher;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            try
            {
                var methodCallCommand = new MethodCallCommand(targetMethod.Name, args);
                var task = new TaskCompletionSource<object>();
                using (_messagesObservable.OfType<MethodCallResultAnswer>()
                    .Where(it => it.AnswerTo == methodCallCommand.Number).Take(1)
                    .Subscribe(it =>
                        {
                            if (it.Exception != null)
                            {
                                task.SetException(it.Exception);
                            }
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
                    return task.Task.Result;
                }
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }
            catch (Exception ex)
            {
                ExceptionDispatchInfo.Capture(ex).Throw();
            }

            return null;
        }

        public static T Create(IObservable<Message> messagesObservable, IPublisher<Message> messagePublisher)
        {
            Guard.IsInterface<T>();
            object proxy = Create<T, ServerProxy<T>>();
            ((ServerProxy<T>) proxy).Initialize(messagesObservable, messagePublisher);
            return (T) proxy;
        }
    }
}