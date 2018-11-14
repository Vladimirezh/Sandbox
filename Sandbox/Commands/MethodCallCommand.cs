using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class MethodCallCommand : Message
    {
        public string MethodName { get; set; }
        public object[] Arguments { get; set; }

        public MethodCallCommand(string methodName, object[] arguments)
        {
            MethodName = methodName;
            Arguments = arguments;
        }
    }
}