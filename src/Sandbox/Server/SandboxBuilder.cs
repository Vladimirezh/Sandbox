using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Sandbox.Common;
using Sandbox.Serializer;

namespace Sandbox.Server
{
    public class SandboxBuilder
    {
        private ISerializer serializer = new BinaryFormatterSerializer();
        private string _address = Guid.NewGuid().ToString();
        private bool createClient;
        private Platform clientPlatform;

        public SandboxBuilder WithSerializer(ISerializer serializer)
        {
            this.serializer = Guard.NotNull(serializer);
            return this;
        }

        public SandboxBuilder WithClient(Platform platform)
        {
            createClient = true;
            clientPlatform = platform;
            return this;
        }

        public SandboxBuilder WithoutClient()
        {
            createClient = false;
            return this;
        }

        public SandboxBuilder WithAddress(string address)
        {
            _address = Guard.NotNullOrEmpty(address, nameof(address));
            return this;
        }

        public Sandbox<TInterface, TObject> Build<TInterface, TObject>() where TObject : class, TInterface, new()
            where TInterface : class
        {
            Guard.IsInterface<TInterface>();

            var server = new NamedPipeServer(new NamedPipedServerFactory(), _address);
            var sandbox = new Sandbox<TInterface, TObject>(server.Select(it => serializer.Deserialize(it)),
                new PublishedMessagesFormatter(server, serializer));
            if (createClient)
                sandbox.AddDisposeHandler(CreateAndRunClient());

            return sandbox;
        }

        private Job.Job CreateAndRunClient()
        {
            var fileName = Path.GetRandomFileName() + ".exe";
            switch (clientPlatform)
            {
                case Platform.x86:
                    File.WriteAllBytes(fileName, Clients.SandboxClient);
                    break;
                case Platform.x64:
                    File.WriteAllBytes(fileName, Clients.SandboxClientx64);
                    break;
                case Platform.AnyCPU:
                    File.WriteAllBytes(fileName, Clients.SandboxClientAnyCPU);
                    break;
            }

            var job = new Job.Job();

            var si = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"\"{_address}\" \"{typeof( EventLoopScheduler ).Assembly.Location}\"",
                FileName = fileName,
                WorkingDirectory = Environment.CurrentDirectory
            };
            job.AddProcess(Process.Start(si).Handle);
            return job;
        }
    }
}