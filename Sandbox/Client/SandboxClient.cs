using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using Sandbox.Commands;

namespace Sandbox.Client
{
    public class SandboxClient : IDisposable
    {
        private readonly IPublisher<Message> _publisher;
        private readonly EventLoopScheduler _scheduler = new EventLoopScheduler();
        private readonly IDisposable _subscription;
        private object _instance;

        public SandboxClient(IObservable<Message> messages, IPublisher<Message> publisher)
        {
            _publisher = publisher;
            _subscription = messages.ObserveOn(_scheduler).Subscribe(ExecuteCommands);
        }

        private void ExecuteCommands(Message message)
        {
            switch (message)
            {
                case CreateObjectOfTypeCommad co:
                    Assembly.Load(co.AssemblyPath);
                    _instance = Activator.CreateInstance(Type.GetType(co.TypeFullName));
                    break;
                case MethodCallCommand mcc:
                    try
                    {
                        var method = _instance.GetType()
                            .GetMethod(mcc.MethodName, mcc.Arguments.Select(it => it.GetType()).ToArray());
                        _publisher.Publish(new MethodCallResultAnswer
                            {AnswerTo = message.Number, Result = method.Invoke(_instance, mcc.Arguments)});
                    }
                    catch (Exception ex)
                    {
                        _publisher.Publish(new MethodCallResultAnswer {Exception = ex});
                    }

                    break;
            }
        }

        public void Dispose()
        {
            _scheduler?.Dispose();
            _subscription?.Dispose();
        }
    }
}