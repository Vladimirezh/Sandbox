using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class EventInvokeCommand : EventCommand
    {
        public object[] Args { get; set; }
    }
}