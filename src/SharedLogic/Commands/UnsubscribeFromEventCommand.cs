using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class UnsubscribeFromEventCommand : EventCommand
    {
        public UnsubscribeFromEventCommand()
        {
        }

        public UnsubscribeFromEventCommand( int number ) : base( number )
        {
        }
    }
}