namespace Sandbox.Commands
{
    public class MethodCallInvokeResultAnswer : Message
    {
        public object Result { get; set; }
        public int AnswerTo { get; set; }
    }
}