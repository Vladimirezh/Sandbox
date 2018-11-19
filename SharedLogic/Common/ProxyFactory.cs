/*using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Castle.DynamicProxy;
using NProxy.Core;

namespace Sandbox.Common
{
    public static class ProxyFactory
    {
        private static readonly Dictionary<Type, IProxyTemplate> templates = new Dictionary<Type, IProxyTemplate>();
        private static readonly NProxy.Core.ProxyFactory proxyFactory = new NProxy.Core.ProxyFactory();
        private static readonly object locker = new object();

        public static TInstance GetProxy<TInstance>(IInvocationHandler handler)
        {
            
                
            var declaringType = typeof(TInstance);
            if (!templates.ContainsKey(declaringType))
            {
                lock (locker)
                {
                    if (!templates.ContainsKey(declaringType))
                    {
                        templates[declaringType] = proxyFactory.GetProxyTemplate(declaringType, new[] {declaringType});
                    }
                }
            }

            return (TInstance) templates[declaringType].CreateProxy(handler);
        }
    }
}*/