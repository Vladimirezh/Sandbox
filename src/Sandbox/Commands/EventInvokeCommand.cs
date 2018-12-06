using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class EventInvokeCommand : EventCommand
    {
        public EventInvokeCommand()
        {
        }

        public EventInvokeCommand( int number ) : base( number )
        {
        }

        public object[] Arguments { get; set; }
    }
}