using System;

namespace Sandbox.Commands
{
    [Serializable]
    public abstract class EventCommand : Message
    {
        protected EventCommand()
        {
        }

        protected EventCommand( int number ) : base( number )
        {
        }

        public string EventName { get; set; }
    }
}