using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class TerminateCommand : Message
    {
        public TerminateCommand()
        {
        }

        public TerminateCommand( int number ) : base( number )
        {
        }
    }
}