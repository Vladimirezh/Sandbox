using Sandbox.Client;
using System;

namespace SandboxClient
{
    public static class Program
    {
        static Sandbox.Client.SandboxClient client;
        static void Main(string[] args)
        {
            client = new SandboxClientBuilder(args[0]).Build();
            Console.ReadKey();
        }
    }
}
