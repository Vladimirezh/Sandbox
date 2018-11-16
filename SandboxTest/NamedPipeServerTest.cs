using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Sandbox;
using SandboxTest.Common;
using Xunit;

namespace SandboxTest
{
    public class TestNamedPipeServer
    {
        private const string NotEmptyAddress = "test";
        private const string SimpleMessage = "SimpleMessage";


        [Theory]
        [InlineData(NotEmptyAddress)]
        [InlineData(NotEmptyAddress + NotEmptyAddress)]
        public void TestNotEmptyAddress(string address)
        {
            var server = new NamedPipeServer(Mock.Of<INamedPipeStreamFactory>(), address);
            Assert.Equal(address, server.Address);
        }

        [Fact]
        public void TestEmptyAddressMustThrowArgEx()
        {
            Assert.Throws<ArgumentException>(() =>
                new NamedPipeServer(Mock.Of<INamedPipeStreamFactory>(), string.Empty));
        }

        [Fact]
        public void TestNullAddressMustThrowArgNullEx()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new NamedPipeServer(Mock.Of<INamedPipeStreamFactory>(), null));
        }


        [Fact]
        public void TestNullFactoryMustThrowArgNullEx()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new NamedPipeServer(null, NotEmptyAddress));
        }

        [Fact]
        public void TestCanSubscribeOnMessagesMustCallConnectWithCorrectAddress()
        {
            var stream = new Mock<INamedPipeStream>();
            stream.Setup(it =>
                    it.ConnectionAsync(It.Is<CancellationToken>(ct => ct != null && ct != CancellationToken.None)))
                .Returns(Task.CompletedTask);
            var streamFactory = new Mock<INamedPipeStreamFactory>();
            streamFactory.Setup(it => it.CreateStream(It.Is<string>(a => a == NotEmptyAddress))).Returns(stream.Object);

            using (new NamedPipeServer(streamFactory.Object, NotEmptyAddress).Subscribe(it => { }))
            {
                streamFactory.Verify(it => it.CreateStream(It.IsAny<string>()), Times.Once);
                streamFactory.VerifyAll();
                stream.VerifyAll();
            }
        }

        [Fact]
        public void TestConnectMustCallOnceOnMultiSubscription()
        {
            var stream = new Mock<INamedPipeStream>();
            stream.Setup(it => it.ConnectionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var streamFactory = new Mock<INamedPipeStreamFactory>();
            streamFactory.Setup(it => it.CreateStream(It.IsAny<string>())).Returns(stream.Object);

            var namedPipeServer = new NamedPipeServer(streamFactory.Object, NotEmptyAddress);
            using (namedPipeServer.Subscribe(it => { }))
            using (namedPipeServer.Subscribe(it => { }))
            using (namedPipeServer.Subscribe(it => { }))
            {
                streamFactory.Verify(it => it.CreateStream(It.IsAny<string>()), Times.Once);
            }
        }

        [Theory]
        [InlineData("This is TestMassage")]
        [InlineData("This is second TestMassage")]
        [InlineData("Test of protocol")]
        public void TestProtocolInFirstReadLengthThenMessage(string messageString)
        {
            var message = Encoding.UTF8.GetBytes(messageString);
            var seq = new MockSequence();
            {
                var stream = new Mock<INamedPipeStream>();
                stream.InSequence(seq).Setup(it => it.ConnectionAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                stream.InSequence(seq).Setup(it => it.ReadAsync(It.Is<byte[]>(ar => ar.Length == sizeof(int)),
                        It.Is<int>(o => o == 0),
                        It.Is<int>(c => c == sizeof(int)),
                        It.Is<CancellationToken>(ct => ct != null && ct != CancellationToken.None)))
                    .Returns<byte[], int, int, CancellationToken>((bytes, offset, count, ct) =>
                    {
                        Array.Copy(BitConverter.GetBytes(message.Length), bytes, bytes.Length);
                        return Task.FromResult(message.Length);
                    });
                stream.InSequence(seq).Setup(it => it.ReadAsync(It.Is<byte[]>(ar => ar.Length == message.Length),
                        It.Is<int>(o => o == 0),
                        It.Is<int>(c => c == message.Length),
                        It.Is<CancellationToken>(ct => ct != null && ct != CancellationToken.None)))
                    .Returns<byte[], int, int, CancellationToken>((bytes, offset, count, ct) =>
                    {
                        Array.Copy(message, bytes, message.Length);
                        return Task.FromResult(message.Length);
                    });
                var streamFactory = new Mock<INamedPipeStreamFactory>();
                streamFactory.Setup(it => it.CreateStream(It.IsAny<string>())).Returns(stream.Object);
                var observer = new Mock<IObserver<byte[]>>();
                observer.Setup(it => it.OnNext(It.Is<byte[]>(msg => message.SequenceEqual(message))));

                using (new NamedPipeServer(streamFactory.Object, NotEmptyAddress).Subscribe(observer.Object))
                {
                    observer.Verify(it => it.OnNext(It.IsAny<byte[]>()), Times.Once);
                    observer.Verify(it => it.OnNext(It.Is<byte[]>(msg => message.SequenceEqual(message))), Times.Once);
                }
            }
        }

        [Fact]
        public void TestExceptionOnCreateStreamMustBePassedToClient()
        {
            var factory = new Mock<INamedPipeStreamFactory>();
            factory.Setup(it => it.CreateStream(It.IsAny<string>())).Throws<TestException>();
            var observer = new Mock<IObserver<byte[]>>();
            using (new NamedPipeServer(factory.Object, NotEmptyAddress).Subscribe(observer.Object))
            {
                observer.Verify(it => it.OnError(It.Is<TestException>(ex => true)), Times.Once);
            }
        }

        [Fact]
        public void TestExceptionOnConnectMustBePassedToClient()
        {
            var stream = new Mock<INamedPipeStream>();
            stream.Setup(it =>
                    it.ConnectionAsync(It.Is<CancellationToken>(ct => ct != null && ct != CancellationToken.None)))
                .Throws<TestException>();
            var streamFactory = new Mock<INamedPipeStreamFactory>();
            streamFactory.Setup(it => it.CreateStream(It.IsAny<string>())).Returns(stream.Object);
            var observer = new Mock<IObserver<byte[]>>();
            using (new NamedPipeServer(streamFactory.Object, NotEmptyAddress).Subscribe(observer.Object))
            {
                observer.Verify(it => it.OnError(It.Is<TestException>(ex => true)), Times.Once);
            }
        }

        [Fact]
        public void TestReadLengthWithZeroByteCountMustCallComplete()
        {
            var stream = new Mock<INamedPipeStream>();
            stream.Setup(it => it.ConnectionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            stream.Setup(it => it.ReadAsync(It.IsAny<byte[]>(), 0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            var streamFactory = new Mock<INamedPipeStreamFactory>();
            streamFactory.Setup(it => it.CreateStream(It.IsAny<string>())).Returns(stream.Object);
            var observer = new Mock<IObserver<byte[]>>();
            using (new NamedPipeServer(streamFactory.Object, NotEmptyAddress).Subscribe(observer.Object))
            {
                observer.Verify(it => it.OnCompleted(), Times.Once);
            }
        }

        [Fact]
        public void TestReadMessageWithZeroByteCountMustCallComplete()
        {
            var seq = new MockSequence();
            var stream = new Mock<INamedPipeStream>();
            stream.InSequence(seq).Setup(it => it.ConnectionAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            stream.InSequence(seq).Setup(it =>
                    it.ReadAsync(It.IsAny<byte[]>(), 0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns<byte[], int, int, CancellationToken>((bytes, offset, count, ct) =>
                {
                    var message = Encoding.UTF8.GetBytes("str");
                    Array.Copy(message, bytes, message.Length);
                    return Task.FromResult(message.Length);
                });
            stream.InSequence(seq)
                .Setup(it => it.ReadAsync(It.IsAny<byte[]>(), 0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            var streamFactory = new Mock<INamedPipeStreamFactory>();
            streamFactory.Setup(it => it.CreateStream(It.IsAny<string>())).Returns(stream.Object);
            var observer = new Mock<IObserver<byte[]>>();
            using (new NamedPipeServer(streamFactory.Object, NotEmptyAddress).Subscribe(observer.Object))
            {
                observer.Verify(it => it.OnCompleted());
            }
        }

        [Fact]
        public void TestUnsubscribeMustCallOnComplete()
        {
            var stream = new Mock<INamedPipeStream>();
            stream.Setup(it =>
                    it.ConnectionAsync(It.Is<CancellationToken>(ct => ct != null && ct != CancellationToken.None)))
                .Returns(Task.CompletedTask);
            var streamFactory = new Mock<INamedPipeStreamFactory>();
            streamFactory.Setup(it => it.CreateStream(It.IsAny<string>())).Returns(stream.Object);
            var observer = new Mock<IObserver<byte[]>>();
            using (new NamedPipeServer(streamFactory.Object, NotEmptyAddress).Subscribe(observer.Object))
            {
            }

            observer.Verify(it => it.OnCompleted(), Times.Once);
        }

        [Theory]
        [InlineData(SimpleMessage)]
        [InlineData(NotEmptyAddress)]
        public void TestPublishMessage(string message)
        {
            var (stream, namedPipeServer) = CreateStreamAndServerForPublishTest();
            using (namedPipeServer)
            using (namedPipeServer.Subscribe(it => { }))
            {
                var messageArray = Encoding.UTF8.GetBytes(message);
                namedPipeServer.Publish(messageArray);
                VerifyMessagePublishing(messageArray, stream);
            }
        }

        [Theory]
        [InlineData(SimpleMessage)]
        [InlineData(NotEmptyAddress)]
        public void TestMessagesMustBePublishedAfterSubscribe(string message)
        {
            var (stream, namedPipeServer) = CreateStreamAndServerForPublishTest();
            using (namedPipeServer)
            {
                var messageArray = Encoding.UTF8.GetBytes(message);
                namedPipeServer.Publish(messageArray);
                using (namedPipeServer.Subscribe(it => { }))
                    VerifyMessagePublishing(messageArray, stream);
            }
        }

        private static void VerifyMessagePublishing(byte[] messageArray, Mock<INamedPipeStream> stream)
        {
            var sizeOfMessage = messageArray.Length + sizeof(int);
            stream.Verify(it =>
                it.WriteAsync(
                    It.Is<byte[]>(arr => arr.Length == sizeOfMessage && arr
                                             .SequenceEqual(BitConverter.GetBytes(messageArray.Length)
                                                 .Concat(messageArray))),
                    0,
                    sizeOfMessage, It.IsAny<CancellationToken>()), Times.Once);
        }

        private (Mock<INamedPipeStream> streamMock, NamedPipeServer server ) CreateStreamAndServerForPublishTest()
        {
            var stream = new Mock<INamedPipeStream>();
            stream.Setup(it =>
                    it.ConnectionAsync(It.Is<CancellationToken>(ct => ct != null && ct != CancellationToken.None)))
                .Returns(Task.CompletedTask);
            stream.Setup(it => it.WriteAsync(It.IsAny<byte[]>(), 0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            stream.Setup(it => it.ReadAsync(It.IsAny<byte[]>(), 0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns<byte[], int, int, CancellationToken>(async (bytes, i, c, ct) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), ct);
                    return 0;
                });

            var streamFactory = new Mock<INamedPipeStreamFactory>();
            streamFactory.Setup(it => it.CreateStream(It.IsAny<string>())).Returns(stream.Object);
            return (stream, new NamedPipeServer(streamFactory.Object, NotEmptyAddress));
        }
    }
}