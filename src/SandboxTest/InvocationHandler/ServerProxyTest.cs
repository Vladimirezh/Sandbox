using System;
using System.Reactive.Subjects;
using Moq;
using Sandbox;
using Sandbox.Commands;
using Sandbox.InvocationHandlers;
using SandboxTest.Common;
using SandboxTest.Instances;
using Xunit;

namespace SandboxTest.InvocationHandler
{
    public class ServerProxyTest
    {
        public ServerProxyTest()
        {
            answerCallback = PostEmptyAnswerTo;
            publisher = new Mock< IPublisher< Message > >();
            publisher.Setup( it => it.Publish( It.IsAny< Message >() ) ).Callback< Message >( it => answerCallback?.Invoke( it ) );

            instance = ServerProxy< ITestClass >.Create( commandsObservable, publisher.Object );
        }

        private Action< Message > answerCallback;
        private readonly Subject< Message > commandsObservable = new Subject< Message >();
        private readonly ITestClass instance;
        private readonly Mock< IPublisher< Message > > publisher;

        private void PostEmptyAnswerTo( Message message )
        {
            commandsObservable.OnNext( new MethodCallResultAnswer { AnswerTo = message.Number } );
        }

        [Fact]
        public void TestCallVoidMethodWithoutParametersTest()
        {
            instance.VoidMethod();
            publisher.Verify( it => it.Publish( It.Is< MethodCallCommand >( c => c.Arguments.Length == 0 && c.MethodName == nameof( instance.VoidMethod ) ) ), Times.Once() );
        }

        [Fact]
        public void TestCallVoidMethodWithParametersTest()
        {
            const string param1 = "Param1";
            const int param2 = 2;
            const float param3 = 3f;

            instance.VoidMethodWithParameters( param1, param2, param3 );
            publisher.Verify(
                it => it.Publish( It.Is< MethodCallCommand >( c => ( string ) c.Arguments[ 0 ] == param1 && ( int ) c.Arguments[ 1 ] == param2 && Math.Abs( ( float ) c.Arguments[ 2 ] - param3 ) < 0.00001
                                                                   && c.MethodName == nameof( instance.VoidMethodWithParameters ) ) ), Times.Once() );
        }

        [Fact]
        public void TestCallMethodWithReturnValueWithoutParameters()
        {
            const int intResult = 5;
            answerCallback = message => commandsObservable.OnNext( new MethodCallResultAnswer { AnswerTo = message.Number, Result = intResult } );
            Assert.Equal( intResult, instance.ReturnIntValue() );
        }

        [Fact]
        public void TestCallMethodWithThrowException()
        {
            answerCallback = message => commandsObservable.OnNext( new MethodCallResultAnswer { AnswerTo = message.Number, Exception = new TestException() } );
            Assert.Throws< TestException >( () => instance.VoidMethod() );
        }
    }
}