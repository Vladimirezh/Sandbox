using System;

namespace Sandbox.Common
{
    public sealed class CantResolveAssemblyException : Exception
    {
        public CantResolveAssemblyException( string assemblyName, string name )
        {
            AssemblyName = assemblyName;
            Name = name;
        }

        public string AssemblyName { get; }
        public string Name { get; }
    }
}