using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class UnexpectedExceptionMessage : Message
    {
        public Exception Exception { get; set; }
    }
}