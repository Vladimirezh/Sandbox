using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class MethodCallResultAnswer : Message
    {
        public object Result { get; set; }
        public int AnswerTo { get; set; }
    }
}