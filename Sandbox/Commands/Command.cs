using System;
using System.Threading;

namespace Sandbox.Commands
{
    [Serializable]
    public class Command
    {
        private static volatile int commandNumber = 0;

        public Command()
        {
            Number = Interlocked.Increment(ref commandNumber);
        }

        public int Number { get; }
    }
}