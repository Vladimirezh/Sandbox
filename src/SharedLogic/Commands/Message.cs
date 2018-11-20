using System;
using System.Threading;

namespace Sandbox.Commands
{
    [Serializable]
    public class Message
    {
        private static volatile int messageNumber = 0;
        private volatile int number;
        public Message()
        {
            number = Interlocked.Increment(ref messageNumber);
        }

        public int Number
        {
            get => number;
            private set { number = value; }
        }
    }
}