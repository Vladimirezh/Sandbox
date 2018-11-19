using System;
using System.Threading;

namespace Sandbox.Commands
{
    [Serializable]
    public class Message
    {
        private static volatile int messageNumber = 0;

        public Message()
        {
            Number = Interlocked.Increment(ref messageNumber);
        }

        public int Number { get; }
    }
}