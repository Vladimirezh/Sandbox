using System;
using System.Runtime.Remoting.Messaging;

namespace Sandbox.InvocationHandlers
{
    internal static class InvocationHandlersExtensions
    {
        public static bool IsEvent( this IMethodCallMessage msm )
        {
            return msm.IsSubscribeToEvent() || msm.IsUnsubscribeFromEvent();
        }

        public static bool IsSubscribeToEvent( this IMethodCallMessage msm )
        {
            return msm.MethodName.StartsWith( "add_", StringComparison.Ordinal );
        }

        public static string GetEventName( this IMethodCallMessage msm )
        {
            return msm.MethodName.Replace( "add_", string.Empty ).Replace( "remove_", string.Empty );
        }

        public static bool IsUnsubscribeFromEvent( this IMethodCallMessage msm )
        {
            return msm.MethodName.StartsWith( "remove_", StringComparison.Ordinal );
        }
    }
}