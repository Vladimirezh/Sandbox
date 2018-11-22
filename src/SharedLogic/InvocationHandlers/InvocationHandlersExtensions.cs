using System;
using System.Runtime.Remoting.Messaging;

namespace SharedLogic.InvocationHandlers
{
    internal static class InvocationHandlersExtensions
    {
        public static bool IsEvent( this IMethodCallMessage msm )
        {
            return msm.MethodName.StartsWith( "add_", StringComparison.Ordinal ) || msm.MethodName.StartsWith( "remove_", StringComparison.Ordinal );
        }
    }
}