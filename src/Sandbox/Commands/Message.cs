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
            _number = Interlocked.Increment( ref messageNumber );
        }

        public Message( int number )
        {
            _number = number;
        }

        private volatile int _number;

        public int Number
        {
            get => _number;
        }
    }
}