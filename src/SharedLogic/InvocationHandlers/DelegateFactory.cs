using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Sandbox.InvocationHandlers
{
    public static class DelegateFactory
    {
        public static Delegate Create( EventInfo e, Action< string, object[] > actionToCall )
        {
            try
            {
                var parameters = e.EventHandlerType.GetMethod("Invoke").GetParameters().Select(p => Expression.Parameter(p.ParameterType, p.Name)).ToList();
                var exp = Expression.Call(Expression.Constant(actionToCall), actionToCall.GetType().GetMethod("Invoke"), Expression.Constant(e.Name), Expression.NewArrayInit(typeof(object), parameters));
                var lambda = Expression.Lambda(exp, parameters);
                return Delegate.CreateDelegate(e.EventHandlerType, lambda.Compile(), "Invoke", false);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}