using System;

namespace Sandbox.Common
{
    public sealed class CantResolveAssemblyException : Exception
    {
        public string AssemblyName { get; }
        public string Name { get; }

        public CantResolveAssemblyException(string assemblyName, string name)
        {
            AssemblyName = assemblyName;
            Name = name;
        }
    }
}