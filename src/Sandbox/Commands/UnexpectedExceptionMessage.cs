using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class UnexpectedExceptionMessage : Message
    {
        public UnexpectedExceptionMessage()
        {
        }

        public UnexpectedExceptionMessage( int number ) : base( number )
        {
        }

        public Exception Exception { get; set; }
    }
}