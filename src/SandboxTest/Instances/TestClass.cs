using System;

namespace SandboxTest.Instances
{
    public class TestClass : ITestClass
    {
        public void VoidMethod()
        {
            throw new NotImplementedException();
        }

        public void VoidMethodWithParameters( string param1, int param2, float param3 )
        {
            throw new NotImplementedException();
        }

        public int ReturnIntValue()
        {
            throw new NotImplementedException();
        }
    }
}