using System;
using System.Threading;

namespace Sandbox.Commands
{
    [Serializable]
    public class Message
    {
        private static int messageNumber;

        public Message()
        {
            number = Interlocked.Increment( ref messageNumber );
        }

        private volatile int number;

        public int Number
        {
            get => number;
        }
    }
}