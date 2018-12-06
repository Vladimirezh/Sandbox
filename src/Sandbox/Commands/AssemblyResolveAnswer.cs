using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class AssemblyResolveAnswer : Message
    {
        public AssemblyResolveAnswer()
        {
        }

        public AssemblyResolveAnswer( int number ) : base( number )
        {
        }

        public int AnswerTo { get; set; }
        public bool Handled { get; set; }
        public string Location { get; set; }
    }
}