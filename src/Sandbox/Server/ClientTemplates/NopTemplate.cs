using System;

namespace Sandbox.Server.ClientTemplates
{
    public class NopTemplate : IClientTemplate
    {
        public IDisposable Run( string address )
        {
            return null;
        }
    }
}