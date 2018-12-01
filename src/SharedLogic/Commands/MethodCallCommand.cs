using System;
using Sandbox.Common;

namespace Sandbox.Commands
{
    [Serializable]
    public class MethodCallCommand : Message
    {
        public MethodCallCommand( string methodName, object[] arguments, string methodId )
        {
            MethodName = Guard.NotNullOrEmpty( methodName, nameof( methodName ) );
            Arguments = arguments;
            MethodId = methodId;
        }

        public string MethodId { get; set; }
        public string MethodName { get; set; }
        public object[] Arguments { get; set; }
    }
}