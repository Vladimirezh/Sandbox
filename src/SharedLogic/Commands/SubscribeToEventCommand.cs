using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class SubscribeToEventCommand : EventCommand
    {
        public SubscribeToEventCommand()
        {
        }

        public SubscribeToEventCommand( int number ) : base( number )
        {
        }
    }
}