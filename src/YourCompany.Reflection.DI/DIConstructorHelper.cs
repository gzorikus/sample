using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace YourCompany.Reflection.DI
{
    public static class DIConstructorHelper
    {
        private static readonly Expression<Action<IServiceProvider>> GetRequiredServiceMethod
            = provider => provider.GetRequiredService<object>();

        private static readonly MethodInfo GetRequiredServiceMethodInfo
            = GetMemberHelper.GetMethodInfo(GetRequiredServiceMethod).GetGenericMethodDefinition();

        public static Func<IServiceProvider, object> CompileRequiredArgumentsFactory(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null) throw new ArgumentNullException(nameof(constructorInfo));
            var parameter = Expression.Parameter(typeof(IServiceProvider), "provider");
            var constructorParameters = constructorInfo.GetParameters();
            var argumentExpressions = new Expression[constructorParameters.Length];
            for (int i = 0; i < constructorParameters.Length; i++)
            {
                var method = GetRequiredServiceMethodInfo.MakeGenericMethod(constructorParameters[i].ParameterType);
                argumentExpressions[i] = Expression.Call(method, parameter);
            }
            var constructorCall = Expression.New(constructorInfo, argumentExpressions);
            return Expression.Lambda<Func<IServiceProvider, object>>(constructorCall, parameter).Compile();
        }
    }
}