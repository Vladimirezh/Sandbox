using System;

namespace Sandbox.Client.TerminatePolicy
{
    public class ExitPolicy : ITerminatePolicy
    {
        public void Terminate()
        {
            Environment.Exit( 0 );
        }
    }
}