namespace Sandbox.Commands
{
    public class AssemblyResolveAnswer : Message
    {
        public int AnswerTo { get; set; }
        public bool Handled { get; set; }
        public string Location { get; set; }
    }
}