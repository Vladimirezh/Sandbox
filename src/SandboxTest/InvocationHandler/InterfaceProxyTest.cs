using System;
using System.Linq.Expressions;
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
    public class InterfaceProxyTest
    {
        public InterfaceProxyTest()
        {
            answerCallback = PostEmptyAnswerTo;
            publisher = new Mock< IPublisher< Message > >();
            publisher.Setup( it => it.Publish( It.IsAny< Message >() ) ).Callback< Message >( it => answerCallback?.Invoke( it ) );

            _callHandler = CallHandler.CreateHandlerFor< ITestClass >( commandsObservable, publisher.Object );
            instance = InterfaceProxy< ITestClass >.Create( _callHandler );
        }

        private Action< Message > answerCallback;
        private readonly Subject< Message > commandsObservable = new Subject< Message >();
        private readonly ITestClass instance;
        private readonly Mock< IPublisher< Message > > publisher;
        private readonly CallHandler _callHandler;

        private void PostEmptyAnswerTo( Message message )
        {
            commandsObservable.OnNext( new MethodCallResultAnswer { AnswerTo = message.Number } );
        }

        [Fact]
        public void TestSingleSubscriptionToAction()
        {
            instance.EventAction += () => { };
            publisher.Verify( it => it.Publish( It.Is< SubscribeToEventCommand >( msg => msg.EventName == nameof( ITestClass.EventAction ) ) ), Times.Once );
        }

        [Fact]
        public void TestMultipleSubscriptionsToAction()
        {
            Action instanceOnEventAction = () => { };
            for ( var i = 0; i < 10; i++ )
                instance.EventAction += instanceOnEventAction;

            publisher.Verify( it => it.Publish( It.Is< SubscribeToEventCommand >( msg => msg.EventName == nameof( ITestClass.EventAction ) ) ), Times.Once );
        }

        [Fact]
        public void TestUnsubscribeFromEvent()
        {
            Action instanceOnEventAction = () => { };
            for ( var i = 0; i < 10; i++ )
                instance.EventAction += instanceOnEventAction;

            for ( var i = 0; i < 9; i++ )
                instance.EventAction -= instanceOnEventAction;

            Expression< Action< IPublisher< Message > > > unsubCommand = it => it.Publish( It.Is< UnsubscribeFromEventCommand >( msg => msg.EventName == nameof( ITestClass.EventAction ) ) );
            publisher.Verify( unsubCommand, Times.Never );
            instance.EventAction -= instanceOnEventAction;
            publisher.Verify( unsubCommand, Times.Once );
        }

        [Fact]
        public void TestInvokeEventHandler()
        {
            var args = new object[] { "sender", new EventArgs() };
            var result = Assert.Raises< EventArgs >( h => instance.EventWithHandler += h, h => instance.EventWithHandler -= h,
                () => _callHandler.HandleMessage( null, new EventInvokeCommand { EventName = nameof( ITestClass.EventWithHandler ), Args = args } ) );
            Assert.Equal( args[ 0 ], result.Sender );
            Assert.Equal( args[ 1 ], result.Arguments );
        }

        [Fact]
        public void TestSingleSubscriptionToEventHandler()
        {
            instance.EventWithHandler += ( sender, s ) => { };
            publisher.Verify( it => it.Publish( It.Is< SubscribeToEventCommand >( msg => msg.EventName == nameof( ITestClass.EventWithHandler ) ) ), Times.Once );
        }

        [Fact]
        public void TestMultipleSubscriptionsToEventHandler()
        {
            for ( var i = 0; i < 10; i++ )
                instance.EventWithHandler += ( sender, s ) => { };
            publisher.Verify( it => it.Publish( It.Is< SubscribeToEventCommand >( msg => msg.EventName == nameof( ITestClass.EventWithHandler ) ) ), Times.Once );
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