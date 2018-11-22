using System;
using Sandbox.Common;

namespace Sandbox.Commands
{
    [Serializable]
    public class MethodCallCommand : Message
    {
        public MethodCallCommand( string methodName, object[] arguments )
        {
            MethodName = Guard.NotNullOrEmpty( methodName, nameof( methodName ) );
            Arguments = arguments;
        }

        public string MethodName { get; set; }
        public object[] Arguments { get; set; }
    }
}