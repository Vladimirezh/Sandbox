using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Moq;
using Sandbox;
using Sandbox.Commands;
using Sandbox.InvocationHandlers;
using SandboxTest.Instances;
using Xunit;

namespace SandboxTest.InvocationHandler
{
    public class ServerInvocationHandlerTest
    {
        private Mock<IPublisher<Message>> publisher;
        private ServerInvocationHandler serverInvocationHandler;
        private Subject<Message> commandsObservable = new Subject<Message>();
        private ITestClass instance;
        private Action<Message> answerCallback;

        public ServerInvocationHandlerTest()
        {
            answerCallback = PostEmptyAnswerTo;
            publisher = new Mock<IPublisher<Message>>();
            publisher.Setup(it => it.Publish(It.IsAny<Message>())).Callback<Message>(it => answerCallback?.Invoke(it));

            serverInvocationHandler = new ServerInvocationHandler(commandsObservable, publisher.Object);
            instance = new ProxyGenerator().CreateInterfaceProxyWithoutTarget<ITestClass>(serverInvocationHandler);
        }

        private void PostEmptyAnswerTo(Message message)
        {
            commandsObservable.OnNext(new MethodCallInvokeResultAnswer {AnswerTo = message.Number});
        }

        [Fact]
        public void TestCallVoidMethodWithoutParametersTest()
        {
            instance.VoidMethod();
            publisher.Verify(it =>
                    it.Publish(It.Is<MethodCallCommand>(c =>
                        c.Arguments.Length == 0 && c.MethodName == nameof(instance.VoidMethod))),
                Times.Once());
        }

        [Fact]
        public void TestCallVoidMethodWithParametersTest()
        {
            var param1 = "Param1";
            var param2 = 2;
            var param3 = 3f;

            instance.VoidMethodWithParameters(param1, param2, param3);
            publisher.Verify(it =>
                    it.Publish(It.Is<MethodCallCommand>(c =>
                        (string) c.Arguments[0] == param1 && (int) c.Arguments[1] == param2 &&
                        (float) c.Arguments[2] == param3 &&
                        c.MethodName == nameof(instance.VoidMethodWithParameters))),
                Times.Once());
        }

        [Fact]
        public void TestCallMethodWithReturnValueWithoutParameters()
        {
            const int intResult = 5;
            answerCallback = message => commandsObservable.OnNext(new MethodCallInvokeResultAnswer
                {AnswerTo = message.Number, Result = intResult});
            Assert.Equal(intResult, instance.ReturnIntValue());
        }
    }
}