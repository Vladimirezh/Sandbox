using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class AssemblyResolveMessage : Message
    {
        public string RequestingAssemblyFullName { get; set; }
        public string Name { get; set; }
    }
}