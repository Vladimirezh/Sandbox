using System;
using System.IO.Pipes;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Commands;
using Sandbox.Common;
using Sandbox.Serializer;

namespace Sandbox.Client
{
    public class SandboxClientBuilder
    {
        private readonly string _address;
        private ISerializer serializer = new BinaryFormatterSerializer();

        public SandboxClientBuilder(string address)
        {
            _address = address;
        }

        public SandboxClientBuilder WithSerializer(ISerializer serializer)
        {
            this.serializer = Guard.NotNull(serializer);
            return this;
        }

        public SandboxClient Build()
        {
            var server = new NamedPipeServer(new NamedPipedClientFactory(), _address);

            var publisher = new PublishedMessagesFormatter(server, serializer);
            var observable = server.Select(it => serializer.Deserialize(it));
            AppDomain.CurrentDomain.UnhandledException += (s, e) => CurrentDomainOnUnhandledException(e, publisher);
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) => ResolveAssembly(e, observable, publisher);
            observable.OfType<TerminateCommand>().Subscribe(it => Environment.Exit(0));

            return new SandboxClient(observable, publisher);
        }

        private Assembly ResolveAssembly(ResolveEventArgs args, IObservable<Message> observable,
            PublishedMessagesFormatter publisher)
        {
            Console.WriteLine(args.RequestingAssembly);
            if (args.RequestingAssembly == null)
                return null;
            var resolveMessage = new AssemblyResolveMessage
                {RequestingAssemblyFullName = args.RequestingAssembly.FullName, Name = args.Name};
            var task = new TaskCompletionSource<AssemblyResolveAnswer>();
            using (observable.OfType<AssemblyResolveAnswer>()
                .Where(it => it.AnswerTo == resolveMessage.Number).Take(1)
                .Subscribe(it => task.SetResult(it)))
            {
                publisher.Publish(resolveMessage);
                var answer = task.Task.Result;
                if (answer.Handled)
                    return Assembly.LoadFile(answer.Location);
            }

            return null;
        }

        private void CurrentDomainOnUnhandledException(UnhandledExceptionEventArgs e,
            IPublisher<Message> publishedMessagesFormatter)
        {
            publishedMessagesFormatter.Publish(new UnexpectedExceptionMessage
                {Exception = e.ExceptionObject as Exception});
            Environment.Exit(0);
        }
    }

    public class NamedPipedClientFactory : INamedPipeStreamFactory
    {
        public INamedPipeStream CreateStream(string address)
        {
            return new NamedPipeSandboxClientStream(address);
        }

        private class NamedPipeSandboxClientStream : INamedPipeStream
        {
            private readonly NamedPipeClientStream stream;

            public NamedPipeSandboxClientStream(string address)
            {
                stream = new NamedPipeClientStream(".", address, PipeDirection.InOut,
                    PipeOptions.Asynchronous);
            }

            public void Dispose()
            {
                stream.Dispose();
            }

            public Task ConnectionAsync(CancellationToken cancellationToken)
            {
                return Task.Run(() => stream.Connect(), cancellationToken);
                //  return stream.ConnectAsync(cancellationToken);
            }

            public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return stream.ReadAsync(buffer, offset, count, cancellationToken);
            }

            public Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return stream.WriteAsync(buffer, offset, count, cancellationToken);
            }

            public Task FlushAsync(CancellationToken cancellationToken)
            {
                return stream.FlushAsync(cancellationToken);
            }
        }
    }
}