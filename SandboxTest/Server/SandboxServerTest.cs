using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Sandbox;
using Sandbox.Commands;
using Sandbox.Server;
using SandboxTest.Instances;
using Xunit;

namespace SandboxTest.Server
{
    public class SandboxServerTest
    {
        [Fact]
        public void TestServerMustSubscribeToCommands()
        {
            var observableCommands = new Mock<IObservable<Command>>();
            new SandboxServer<ITestClass, TestClass>(observableCommands.Object, Mock.Of<IPublisher<Command>>());
            observableCommands.Verify(it => it.Subscribe(It.IsAny<IObserver<Command>>()), Times.AtLeastOnce);
        }

        [Fact]
        public void TestServerMustPublishSubscribeToUnexpectedExceptions()
        {
            var publisher = new Mock<IPublisher<Command>>();
            new SandboxServer<ITestClass, TestClass>(Mock.Of<IObservable<Command>>(), publisher.Object);
            publisher.Verify(it => it.Publish(It.Is<Command>(c => c is SubscribeToUnexpectedExceptionsCommand)),
                Times.Once);
        }

        [Fact]
        public void TestServerMustPublishCreateObjectOfTypeCommand()
        {
            var publisher = new Mock<IPublisher<Command>>();
            new SandboxServer<ITestClass, TestClass>(Mock.Of<IObservable<Command>>(), publisher.Object);
            publisher.Verify(it => it.Publish(It.Is<Command>(c =>
                c is CreateObjectOfTypeCommad &&
                (c as CreateObjectOfTypeCommad).AssemblyPath == typeof(TestClass).Assembly.Location &&
                (c as CreateObjectOfTypeCommad).TypeFullName == typeof(TestClass).FullName)));
        }

        [Fact]
        public void TestFirstGenericParameterMustBeInterface()
        {
            Assert.Throws<ArgumentException>(() =>
                new SandboxServer<TestClass, TestClass>(Mock.Of<IObservable<Command>>(),
                    Mock.Of<IPublisher<Command>>()));
        }

        [Fact]
        public void TestInstanceMustBeNotNullAfterServerCreation()
        {
            Assert.NotNull(new SandboxServer<ITestClass, TestClass>(Mock.Of<IObservable<Command>>(),
                Mock.Of<IPublisher<Command>>()).Instance);
        }
    }
}