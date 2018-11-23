using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class EventCommand : Message
    {
        public string EventName { get; set; }
    }
}