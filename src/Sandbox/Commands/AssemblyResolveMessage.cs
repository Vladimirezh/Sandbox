using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class AssemblyResolveMessage : Message
    {
        public AssemblyResolveMessage()
        {
        }

        public AssemblyResolveMessage( int number ) : base( number )
        {
        }

        public string RequestingAssemblyFullName { get; set; }
        public string Name { get; set; }
    }
}