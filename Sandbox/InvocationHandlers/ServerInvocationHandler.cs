using System;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Sandbox.Commands;

namespace Sandbox.InvocationHandlers
{
    public class ServerInvocationHandler : IInterceptor
    {
        private readonly IObservable<Message> _messagesObservable;
        private readonly IPublisher<Message> _messagePublisher;

        public ServerInvocationHandler(IObservable<Message> messagesObservable, IPublisher<Message> messagePublisher)
        {
            _messagesObservable = messagesObservable;
            _messagePublisher = messagePublisher;
        }

        public void Intercept(IInvocation invocation)
        {
            var methodCallCommand = new MethodCallCommand(invocation.Method.Name, invocation.Arguments);
            var task = new TaskCompletionSource<object>();
            _messagesObservable.OfType<MethodCallInvokeResultAnswer>()
                .Where(it => it.AnswerTo == methodCallCommand.Number).Take(1)
                .Subscribe(it => task.SetResult(it.Result), ex => task.SetException(ex));
            _messagePublisher.Publish(methodCallCommand);
            invocation.ReturnValue = task.Task.Result;
        }
    }
}