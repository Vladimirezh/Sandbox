using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class MethodCallCommand : Message
    {
        public MethodCallCommand()
        {
        }

        public MethodCallCommand( int number ) : base( number )
        {
        }

        public string MethodId { get; set; }
        public string MethodName { get; set; }
        public object[] Arguments { get; set; }
    }
}