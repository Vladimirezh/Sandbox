using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Moq;
using Sandbox;
using Sandbox.Commands;
using Sandbox.Server;
using SandboxTest.Common;
using SandboxTest.Instances;
using Xunit;

namespace SandboxTest.Server
{
    public class SandboxTest
    {
        [Fact]
        public void TestServerMustSubscribeToCommands()
        {
            var observableCommands = new Mock< IObservable< Message > >();
            new Sandbox< ITestClass, TestClass >( observableCommands.Object, Mock.Of< IPublisher< Message > >() );
            observableCommands.Verify( it => it.Subscribe( It.IsAny< IObserver< Message > >() ), Times.AtLeastOnce );
        }

        [Fact]
        public void TestServerMustPublishCreateObjectOfTypeCommand()
        {
            var publisher = new Mock< IPublisher< Message > >();
            new Sandbox< ITestClass, TestClass >( Mock.Of< IObservable< Message > >(), publisher.Object );
            publisher.Verify( it => it.Publish( It.Is< CreateObjectOfTypeCommand >( c => c.AssemblyPath == typeof( TestClass ).Assembly.Location && c.TypeFullName == typeof( TestClass ).FullName ) ) );
        }

        [Fact]
        public void TestFirstGenericParameterMustBeInterface()
        {
            Assert.Throws< ArgumentException >( () => new Sandbox< TestClass, TestClass >( Mock.Of< IObservable< Message > >(), Mock.Of< IPublisher< Message > >() ) );
        }

        [Fact]
        public void TestInstanceMustBeNotNullAfterServerCreation()
        {
            Assert.NotNull( new Sandbox< ITestClass, TestClass >( Mock.Of< IObservable< Message > >(), Mock.Of< IPublisher< Message > >() ).Instance );
        }

        [Fact]
        public void TestUnexpectedExceptionMessageReceive()
        {
            var messagesObservable = new Subject< Message >();
            var server = new Sandbox< ITestClass, TestClass >( messagesObservable, Mock.Of< IPublisher< Message > >() );
            var exceptions = new List< Exception >();
            server.UnexpectedExceptionHandler.Subscribe( ex => exceptions.Add( ex ) );
            messagesObservable.OnNext( new UnexpectedExceptionMessage { Exception = new TestException() } );
            Assert.Single( exceptions );
            Assert.True( exceptions[ 0 ] is TestException );
        }

        [Fact]
        public void TestRevolveHandledAssemblyMessage()
        {
            var publisher = new Mock< IPublisher< Message > >();
            var messagesObservable = new Subject< Message >();
            new Sandbox< ITestClass, TestClass >( messagesObservable, publisher.Object );
            var assemblyResolveMessage = new AssemblyResolveMessage { RequestingAssemblyFullName = GetType().Assembly.FullName };
            messagesObservable.OnNext( assemblyResolveMessage );
            publisher.Verify( it => it.Publish( It.Is< AssemblyResolveAnswer >( asa => asa.Handled && asa.AnswerTo == assemblyResolveMessage.Number && asa.Location == GetType().Assembly.Location ) ) );
        }

        [Fact]
        public void TestRevolveUnhandledAssemblyMessage()
        {
            var publisher = new Mock< IPublisher< Message > >();
            var messagesObservable = new Subject< Message >();
            new Sandbox< ITestClass, TestClass >( messagesObservable, publisher.Object );
            var assemblyResolveMessage = new AssemblyResolveMessage { RequestingAssemblyFullName = "UnknownAssembly" };
            messagesObservable.OnNext( assemblyResolveMessage );
            publisher.Verify( it => it.Publish( It.Is< AssemblyResolveAnswer >( asa => !asa.Handled && asa.AnswerTo == assemblyResolveMessage.Number && asa.Location == null ) ) );
        }
    }
}