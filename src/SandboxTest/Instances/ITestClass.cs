using System;

namespace SandboxTest.Instances
{
    public interface ITestClass
    {
        void VoidMethod();
        void VoidMethodWithParameters( string param1, int param2, float param3 );
        int ReturnIntValue();
        event Action EventAction;
        event EventHandler< EventArgs > EventWithHandler;
        event EventHandler< EventArgsStruct > EventWithStructArg;
    }
}