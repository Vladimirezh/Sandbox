using System;
using Sandbox.Common;

namespace Sandbox.Commands
{
    [Serializable]
    public class CreateObjectOfTypeCommad : Message
    {
        public CreateObjectOfTypeCommad(string typeFullName, string assemblyPath)
        {
            TypeFullName = Guard.NotNullOrEmpty(typeFullName, nameof(typeFullName));
            AssemblyPath = Guard.NotNullOrEmpty(assemblyPath, nameof(assemblyPath));
        }

        public string AssemblyPath { get; set; }

        public string TypeFullName { get; set; }
    }
}