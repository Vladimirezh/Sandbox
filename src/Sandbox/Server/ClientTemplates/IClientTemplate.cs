using System;

namespace Sandbox.Server.ClientTemplates
{
    public interface IClientTemplate
    {
        IDisposable Run( string address );
    }
}