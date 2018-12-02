using System;

namespace Sandbox.Commands
{
    [Serializable]
    public class MethodCallResultAnswer : Message
    {
        public MethodCallResultAnswer()
        {
        }

        public MethodCallResultAnswer( int number ) : base( number )
        {
        }

        public object Result { get; set; }
        public int AnswerTo { get; set; }
        public Exception Exception { get; set; }
    }
}