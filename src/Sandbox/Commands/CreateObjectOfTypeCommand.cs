using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class CreateObjectOfTypeCommand : Message
    {
        public CreateObjectOfTypeCommand()
        {
        }

        public CreateObjectOfTypeCommand( int number ) : base( number )
        {
        }

        public string AssemblyPath { get; set; }

        public string TypeFullName { get; set; }
    }
}