using System;

namespace Sandbox.Client
{
    public class ExitPolicy : ITerminatePolicy
    {
        public void Terminate()
        {
           Environment.Exit( 0 );
        }
    }
}